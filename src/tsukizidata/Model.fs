module Tukizi

open System
open Newtonsoft.Json

type Sale() =
    let mutable _id = ""
    let mutable _date = DateTime()
    let mutable _section = ""
    let mutable _kind = ""
    let mutable _market = ""
    let mutable _name = ""
    let mutable _method = ""
    let mutable _weight = 0.0

    [<JsonProperty(PropertyName = "Id")>]
    member x.Id with get() = _id and set(v) = _id <- v
    [<JsonProperty(PropertyName = "Date")>]
    member x.Date with get() = _date and set(v) = _date <- v
    [<JsonProperty(PropertyName = "Section")>]
    member x.Section with get() = _section and set(v) = _section <- v
    [<JsonProperty(PropertyName = "Kind")>]
    member x.Kind with get() = _kind and set(v) = _kind <- v
    [<JsonProperty(PropertyName = "Market")>]
    member x.Market with get() = _market and set(v) = _market <- v
    [<JsonProperty(PropertyName = "Name")>]
    member x.Name with get() = _name and set(v) = _name <- v
    [<JsonProperty(PropertyName = "Method")>]
    member x.Method with get() = _method and set(v) = _method <- v
    [<JsonProperty(PropertyName = "Weight")>]
    member x.Weight with get() = _weight and set(v) = _weight <- v

