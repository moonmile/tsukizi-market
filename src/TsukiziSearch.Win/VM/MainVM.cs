using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiziSearch.Win.Models;

namespace TsukiziSearch.Win.VM
{
    class MainVM : ObservableObject
    {
        private const string endpoint = "https://tukizi-market.documents.azure.com:443/";
        private const string apiKey = "G8De15OTD9hAXJdTeHoqOGWF4f1NFHi9W7axcrpMtlHC1FMJo1nFx4VAsO9IdRZ9lUErzL00Aw7g3kiHEqkGyw==";
        private const string databaseId = "Tukizi";
        private const string collectionId = "Sales";

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsSectionFish { get; set; }
        public bool IsSectionVegetable { get; set; }
        public List<string> MarketItems { get; set; }
        public string MarketItem { get; set; }
        public List<string> NameItems { get; set; }
        public string NameItem { get; set; }
        public bool IsKindAll { get; set; }
        public bool IsKindBig { get; set; }
        public bool IsKindFresh { get; set; }
        public bool IsKindLive { get; set; }
        public bool IsKindFrozen { get; set; }
        public bool IsKindProcessing { get; set; }
        public bool IsKindVegitable { get; set; }
        public bool IsKindFruits { get; set; }

        public bool IsMethodAll { get; set; }
        public bool IsMethodSeri { get; set; }
        public bool IsMethodSoutai { get; set; }
        public bool IsMethodOther { get; set; }

        public bool IsCalcSummary { get; set; }
        public bool IsCalcDetail { get; set; }

        private List<Sale> _Items;
        public List<Sale> Items { get => _Items; set => SetProperty(ref _Items, value, nameof(Items)); }
        private string _Message;
        public string Message { get => _Message; set => SetProperty(ref _Message, value, nameof(Message)); }
        public MainVM()
        {
            this.StartDate = DateTime.Now;
            this.EndDate = DateTime.Now;
            this.IsSectionFish = true;
            this.IsKindAll = true;
            this.IsMethodAll = true;
            this.IsCalcDetail = true;
            this.NameItems = new List<string>()
            {
                "すべて",
                "まぐろ（生鮮）",
                "まぐろ（冷凍）",
                "きわだ（生鮮）",
                "きわだ（冷凍）",
                "めばち（生鮮）",
                "めばち（冷凍）",
                "いんど（生鮮）",
                "いんど（冷凍）",
                "まかじき（生鮮）",
                "めかじき（生鮮）",
                "めかじき（冷凍）",
                "その他（大物生）",
                "その他（大物冷）",
                "しばえび",
                "くるまえび",
                "めじ",
                "まだい",
                "きんめだい",
                "あまだい",
                "ぶり・わらさ",
                "いなだ・わかし",
                "はまち",
                "ひらめ",
                "かれい",
                "あじ",
                "さば",
                "かつお",
                "いわし",
                "にしん",
                "さんま",
                "するめいか",
                "やりいか",
                "煮だこ",
                "たら類",
                "すけそう",
                "さけます類",
                "かます",
                "めばる",
                "さわら・さごち",
                "きんき",
                "めぬけ",
                "すずき",
                "いさき",
                "こはだ",
                "まながつお",
                "とびうお",
                "たちうお",
                "あんこう",
                "かき",
                "ほたて",
                "あかがい",
                "うに",
                "けがに",
                "その他（鮮魚）",
                "活またい",
                "活ひらめ",
                "活かんぱち",
                "活あなご",
                "その他（活魚）",
                "冷さば",
                "冷さんま",
                "冷するめ",
                "冷まだい",
                "冷にしん",
                "冷ぎんだら",
                "冷かれい",
                "冷さけ",
                "冷もんごう",
                "冷たこ",
                "冷えび",
                "冷煮ずわい",
                "その他（冷凍）",
                "塩さけ",
                "塩かずのこ",
                "こうなご",
                "しらす干",
                "にぼし",
                "身欠にしん",
                "開干さんま",
                "開干さば",
                "開干かます",
                "開干あじ",
                "めざし",
                "蒲焼うなぎ",
                "ししゃも",
                "煮ほたるいか",
                "すけこ",
                "かまぼこ",
                "ちくわ",
                "あげもの",
                "塩ます",
                "干するめ",
                "その他（塩干加工）",
                "まかじき（冷凍）",
                "きす",
                "たかべ",
                "あゆ",
                "だいこん",
                "かぶ",
                "にんじん",
                "ごぼう",
                "キャベツ",
                "レタス",
                "はくさい",
                "こまつな",
                "ほうれんそう",
                "ねぎ",
                "みずな",
                "セルリー",
                "ブロッコリー",
                "きゅうり",
                "なす",
                "トマト",
                "ピーマン",
                "じゃがいも",
                "さつまいも",
                "さといも",
                "たまねぎ",
                "なましいたけ",
                "その他野菜",
                "みかん",
                "ポンカン",
                "いよかん",
                "はっさく",
                "不知火",
                "きんかん",
                "りんご",
                "いちご",
                "アールスメロン",
                "キーウイ",
                "その他果実",
                "ネーブルオレンジ",
                "あまなつかん",
                "なのはな",
                "かぼちゃ",
                "清見",
                "たけのこ",
                "ふき",
                "そらまめ",
                "アンデスメロン",
                "すいか",
                "こだますいか",
                "プリンスメロン",
                "アスパラガス",
                "さやえんどう",
                "ピース",
                "びわ",
                "さくらんぼ",
                "ぶどう",
                "クインシーメロン",
                "マンゴー",
                "とうもろこし",
                "いんげん",
                "えだまめ",
                "うめ",
                "もも",
                "貴味メロン",
                "オクラ",
                "レイシにがうり",
                "なし",
                "すもも",
                "ネクタリン",
                "いちぢく",
                "まつたけ",
                "くり",
                "西洋なし",
                "れんこん",
                "しゅんぎく",
                "ほしがき",
                "ミニトマト",
                "キウイ",
                "河内晩柑",
                "にら",
                "えのきだけ",
                "しめじ",
            };
            this.NameItem = "すべて";
            this.MarketItems = new List<string>()
            {
                "すべて",
                "築地",
                "足立",
                "大田",
                "豊洲",
                "豊島",
                "淀橋",
                "板橋",
                "世田谷",
                "北足立",
                "多摩NT",
                "葛西",
            };
            this.MarketItem = "すべて";
        }


        /// <summary>
        /// 検索
        /// </summary>
        public async Task Search()
        {
            var client = new DocumentClient(new Uri(endpoint), apiKey);
            // await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
            // await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));

            var query = client.CreateDocumentQuery<Sale>(
                            UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                            new FeedOptions() { MaxItemCount = 1000 })
                        .Where(t => this.StartDate <= t.Date && t.Date <= this.EndDate);

            query = query.Where(t => t.Section == (this.IsSectionFish ? "水産" : "青果"));
            if (this.MarketItem != "すべて") { query = query.Where(t => t.Market == this.MarketItem); }
            if (this.NameItem != "すべて") { query = query.Where(t => t.Name == this.NameItem); }

            var result = query.AsDocumentQuery();

            // 最大10万件に制限する
            var items = new List<Sale>();
            for (int i = 0; i < 100; i++)
            {
                if (result.HasMoreResults == false) break;
                items.AddRange(await result.ExecuteNextAsync<Sale>());
            }

            if ( this.IsCalcSummary == true )
            {
                // 小計を計算する場合
                if (this.IsMethodAll == true)
                {
                    var lst = new List<Sale>();
                    // 販売方法（セリ、相対、第三者等）をひとつにまとめる
                    foreach (var it in items.Select(t => t.Date).Distinct().OrderBy(t => t))
                    {
                        var item = items.FirstOrDefault(t => t.Date == it.Date);
                        if (this.NameItem == "すべて") item.Name = "すべて";
                        if (this.MarketItem == "すべて") item.Market = "すべて";
                        if (this.IsKindAll) item.Kind = "すべて";
                        if ( this.IsMethodAll ) item.Method = "すべて";
                        item.Weight = items.Where(t => t.Date == it.Date).Sum(t => t.Weight);
                        lst.Add(item);
                    }
                    items = lst;
                }
            }
            this.Items = items;
            this.Message = $"検索結果は {items.Count} 件です";
        }
    }
}
