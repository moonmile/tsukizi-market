// Learn more about F# at http://fsharp.org

open System
open System.Net.Http
open System.Text
open System.IO
open System.Text.RegularExpressions
open System.Linq
open System.Linq.Expressions
open Microsoft.Azure.Documents
open Microsoft.Azure.Documents.Client
open Microsoft.Azure.Documents.Linq
open Newtonsoft.Json
open Tukizi

type Config() =
    let mutable _apiKey = ""
    [<JsonProperty(PropertyName = "apiKey")>]
    member x.ApiKey with get() = _apiKey and set(v) = _apiKey <- v


let Endpoint = "https://tukizi-market.documents.azure.com:443/"
/// tukizidata.config から読み込み
let mutable Key = ""
let loadkey() = 
    try
        let path =AppDomain.CurrentDomain.BaseDirectory + "tsukizidata.config"
        let json = System.IO.File.ReadAllText( AppDomain.CurrentDomain.BaseDirectory + "tsukizidata.config" )
        let config = JsonConvert.DeserializeObject<Config>( json )
        config.ApiKey
    with
        | _ -> ""

let DatabaseId = "Tukizi"
let CollectionId = "Sales";


// 1行目: 販売結果（青果・全市場）　→ 棟
// 2行目: 平成30年08月01日(水曜日) → 日付
// "（単位：キロ）" で → 分類
// "品名..." で → 市場
// 以降、1行ごとに 品物, 販売方法, 卸売数量（市場ごと）
// ただし、小計、全分類合計は取り込まない

// 出力フォーマット
// 日付, 棟, 分類, 市場, 品名, 販売方法, 卸売数量

let rec readtable (sr:System.IO.StreamReader) (sw:System.IO.StreamWriter) 棟 (日付:DateTime) 分類 (市場:string[]) 品名_ =
    let 数量 = sr.ReadLine().Split(",")
    let 品名 = if 数量.[0] <> "" then 数量.[0] else 品名_
    if 数量.[0] <> "小計" then
        for i in [4..市場.Length-1] do
            let 市場名 = 市場.[i]
            let 卸売数量 = if 数量.[i] = "－" then "0" else 数量.[i]
            let 販売方法 = 数量.[2]
            sw.WriteLine(  
            // printfn "%s" (
                String.Format("{0},{1},{2},{3},{4},{5},{6}", 
                    日付.ToString("yyyy/MM/dd"), 
                    棟,
                    分類,
                    市場名,
                    品名,
                    販売方法,
                    卸売数量  ))
        readtable sr sw 棟 日付 分類 市場 品名
    ()

let readblock (sr:System.IO.StreamReader) (sw:System.IO.StreamWriter) 棟 日付 =
    let 分類 = sr.ReadLine().Replace("（単位：キロ）","")
    let 市場 = sr.ReadLine().Split(",")
    if 分類 <> "全分類合計" then
        readtable sr sw 棟 日付 分類 市場 ""
    ()

let readcsv (sr:System.IO.StreamReader) (sw:System.IO.StreamWriter) =

    let 棟 = Regex.Replace( sr.ReadLine(), "販売結果（(.+)・全市場）","$1")
    let 日付 = DateTime.Parse( 
                sr.ReadLine().Substring(0,11), 
                new System.Globalization.CultureInfo("ja-JP"), 
                System.Globalization.DateTimeStyles.AssumeLocal)
    sr.ReadLine() |> ignore // 空行
    while sr.EndOfStream = false do
        readblock sr sw 棟 日付
        // 空行まで読み捨て
        while sr.EndOfStream = false && sr.ReadLine() <> "" do
            () 
    ()


/// CSV形式データをダウンロード
let dataget (sec:string) (date:string) =

    // http://www.shijou-nippo.metro.tokyo.jp/SN/201811/20181112/Sui/Sui_K0.csv
    // http://www.shijou-nippo.metro.tokyo.jp/SN/201811/20181112/Sei/Sei_K0.csv

    let url = 
        if sec = "sui" then
            String.Format("http://www.shijou-nippo.metro.tokyo.jp/SN/{0}/{1}/Sui/Sui_K0.csv", date.Substring(0,6), date )
        else
            String.Format("http://www.shijou-nippo.metro.tokyo.jp/SN/{0}/{1}/Sei/Sei_K0.csv", date.Substring(0,6), date )
    let hc = new HttpClient()
    System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
    let enc = System.Text.Encoding.GetEncoding("shift_jis")
    try
        let st = hc.GetStreamAsync(url).Result
        // let sr = (new System.IO.StreamReader( st, enc, true )) :> System.IO.TextReader
        // let text = sr.ReadToEnd()

        let sr = new System.IO.StreamReader( st, enc, true )
        let sw = new System.IO.StreamWriter( String.Format("{0}_{1}.csv", sec, date))
        readcsv sr sw
        sw.Close()
    with
        | _ -> ()   // 無視
    ()


let uploadcsv (sr:System.IO.StreamReader) = 
    let client = new DocumentClient( new Uri(Endpoint), Key )
    let mutable i = 1
    while sr.EndOfStream = false do
        let a = sr.ReadLine().Split(",")
        // CSV形式で読み込み
        let data = Sale( 
                    Id = Guid.NewGuid().ToString("D"),
                    Date = DateTime.Parse(a.[0]), 
                    Section = a.[1], 
                    Kind = a.[2], 
                    Market = a.[3], 
                    Name= a.[4], 
                    Method= a.[5], 
                    Weight= Double.Parse(a.[6]) )
        client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), data).Result |> ignore 
        // printfn "%d %s" i (data.Date.ToString())
        i <- i + 1
    printfn "%d 件登録しました" i
    ()

let dataputcsv (csv:string) =
    // 1. CSVファイルを開く
    let sr = new System.IO.StreamReader( csv )
    // 2. 最初の行を読み込み、日付を取得
    let date = DateTime.Parse(sr.ReadLine().Split(",").[0])
    // 3. Azure cosmos DB で検索
    let client = new DocumentClient( new Uri(Endpoint), Key )
    let query = client.CreateDocumentQuery<Sale>(
                    UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                    new FeedOptions( MaxItemCount = new Nullable<int>( -1 )))
                    .Where( fun t -> t.Date = date )
                    .AsDocumentQuery()
    if query.HasMoreResults = true && query.ExecuteNextAsync<Sale>().Result.Count > 0 then
        // 3.1 マッチすればおしまい
        ()
    else
        // 3.2 マッチしなければ全行アップロードする
        sr.Close()
        let sr = new System.IO.StreamReader( csv )
        uploadcsv sr
        ()
    

/// CSV形式データをアップロード
let dataput (sec:string) (date:string) =
    let csv = String.Format("{0}_{1}.csv", sec, date)
    dataputcsv csv
    


let datadel (sec:string) (date:string) =
    let date = 
        DateTime.Parse(
            String.Format("{0}-{1}-{2}", 
                date.Substring(0,4), 
                date.Substring(4,2), 
                date.Substring(6,2)))
    let client = new DocumentClient( new Uri(Endpoint), Key )
    let query = client.CreateDocumentQuery<Sale>(
                    UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                    new FeedOptions( MaxItemCount = new Nullable<int>( -1 )))
                    .Where( fun t -> t.Date = date )
                    .AsDocumentQuery()
    let mutable i = 0
    while query.HasMoreResults do
        let lst = query.ExecuteNextAsync<Sale>().Result
        i <- i + lst.Count
        for it in lst do  
            client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, it.Id)).Result |> ignore
    printfn "%d 件削除しました" i
    ()




let help() =
    printfn "tskizi-market data tools."
    printfn " tzkizidata get {sui|sei} [20181113]"
    printfn " tzkizidata put {sui|sei} [20181113]"
    printfn " tzkizidata put sei_20181113.csv"
    printfn " tzkizidata del {sui|sei} [20181113]"

[<EntryPoint>]
let main argv =
    if argv.Length > 0 then
        let cmd = argv.[0]
        Key <- loadkey() 
        match cmd with
            | "get" -> 
                    if argv.Length = 2 then
                        dataget argv.[1] (DateTime.Now.ToString("yyyyMMdd"))
                    else
                        dataget argv.[1] argv.[2]
                    ()
            | "put"  -> 
                    if argv.Length = 2 then
                        dataputcsv argv.[1]
                    else
                        dataput argv.[1] argv.[2]
                    ()
            | "del"  -> 
                    if argv.Length = 3 then
                        datadel argv.[1] argv.[2]
                    ()
            | _ -> help()
    else
        help()

    0 // return an integer exit code
