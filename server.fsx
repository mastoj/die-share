#r "./packages/Suave/lib/net40/Suave.dll"
#r "./packages/Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll"
#r "./packages/Zlib.Portable/lib/portable-net4+sl5+wp8+win8+wpa81+MonoTouch+MonoAndroid/Zlib.Portable.dll"
#r "./packages/FSharp.Data/lib/net40/FSharp.Data.dll"

open Suave
open Suave.Successful
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors

// type WebPart = HttpContext -> Async<HttpContext option>

let hello1 = OK "Hello NDC"
let hello2 (ctx:HttpContext) =
    async {
        let host = System.Environment.MachineName
        let responseBytes = System.Text.Encoding.UTF8.GetBytes("Hello world: " + host)
        let response = {
                ctx.response with content = Bytes responseBytes
            }
        do! Async.Sleep 5000
        return (Some {ctx with response = response})
    }

let hello3 (message, idx) = OK (sprintf "Hello %s %i" message idx)

module Logging =
    open System
    let log prefix msg (reqId:Guid) ctx =
        printfn "==> (%A) %s: %s" reqId prefix (msg())
        succeed ctx

    let logTime prefix =
        log prefix (fun () -> System.DateTime.Now.ToString("O"))

    let logUrl reqId (ctx:HttpContext) =
        log "Url" (fun () -> ctx.request.url.AbsolutePath) reqId ctx

    let logStartTime = logTime "Start request"
    let logEndTime = logTime "End request"

    let logRequest part =
        context (fun _ ->
            let reqId = Guid.NewGuid()
            logStartTime reqId >=> logUrl reqId >=> part >=> logEndTime reqId
        )

module JsonHelper =
    open Suave.Json
    open Newtonsoft.Json
    open System.Text

    let utf8GetBytes (str:string) = Encoding.ASCII.GetBytes(str)

    let deserialize<'T> bytes = JsonConvert.DeserializeObject<'T>(Encoding.ASCII.GetString(bytes))
    let serialize<'T> (x:'T) = JsonConvert.SerializeObject(x) |> utf8GetBytes

    let mapJsonNet<'TIn, 'TOut> = mapJsonWith deserialize<'TIn> serialize<'TOut>

module TempPopApi =
    open System
    open JsonHelper
    open FSharp.Data

    type Weather = JsonProvider<"""https://query.yahooapis.com/v1/public/yql?q=select%20item.condition%2C%20location%20from%20weather.forecast%20where%20woeid%20in%20(select%20woeid%20from%20geo.places(1)%20where%20text%3D%22Oslo%22)&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys""">
    let worldBankData = WorldBankData.GetDataContext()

    type GetTempPop = {
        City: string
        Date: DateTime
    }

    type GetTempPopResponse = {
        Request: GetTempPop
        Temp: float option
        Pop: float option
        Country: string option
    }

    let getCurrentTemperaturAndCountry city res =
        try
            let url = sprintf "https://query.yahooapis.com/v1/public/yql?q=select%%20item.condition%%2C%%20location%%20from%%20weather.forecast%%20where%%20woeid%%20in%%20(select%%20woeid%%20from%%20geo.places(1)%%20where%%20text%%3D%%22%s%%22)&format=json&env=store%%3A%%2F%%2Fdatatables.org%%2Falltableswithkeys" city
            let result = Weather.Load(url)
            let tempF = result.Query.Results.Channel.Item.Condition.Temp
            let tempC = float(tempF-32)/1.8
            let country = result.Query.Results.Channel.Location.Country
            {res with Temp = Some tempC; Country = Some country}
        with
        | _ -> res

    let getPopulation res =
        match res.Country, res.Request.Date.Year with
        | Some country, year ->
            try
                worldBankData.Countries
                |> Seq.tryFind(fun x -> x.Name.ToLower() = country.ToLower())
                |> Option.bind(fun country ->
                    let indicator = country.Indicators.``Population, total``
                    match indicator.Years |> Seq.tryFindIndex (fun y -> y = year) with
                    | Some index ->
                        let population =
                            indicator.Values
                            |> Seq.item index
                        Some {res with Pop = Some population}
                    | None -> None)
                |> (function
                    | Some x -> x
                    | None -> res)
            with
            | _ -> res
        | _ -> res

    let getTempPop =
        mapJsonNet(
            fun r ->
                {Request = r; Temp = None; Country = None; Pop = None}
                |> getCurrentTemperaturAndCountry r.City
                |> getPopulation
            )

let app =
    choose [
        path "/" >=> hello1
        path "/hello2" >=> hello2
        pathScan "/hello3/%s/%i" hello3
        path "/api/temppop" >=> TempPopApi.getTempPop
        NOT_FOUND "#WAT"
    ] |> Logging.logRequest

//startWebServer defaultConfig hello2
