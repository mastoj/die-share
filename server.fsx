#load "src/references.fsx"
//#load "app.fsx"

open Suave
open Suave.Filters
open Suave.Successful
open Suave.RequestErrors
open Suave.Operators
open System
//open App
module Logging =
    let logTimestampPart prefix (ctx:HttpContext) =
        async {
            printfn "==> %s: %A" prefix (DateTime.Now.ToString("O"))
            return Some ctx
        }
    let logRequestPart (ctx:HttpContext) =
        async {
            printfn "==> Request: %A" ctx.request.path
            return Some ctx
        }

    let logPart (part:WebPart) =
        logRequestPart
        >=> logTimestampPart "Start"
        >=> part
        >=> logTimestampPart "End"

let app =
    choose [
        path "/hello" >=> (OK "Hello world")
        pathScan "/hello/%i" ((sprintf "Hello %i") >> OK)
        NOT_FOUND (sprintf "ohuh %A" System.DateTime.Now.Ticks)
    ] |> Logging.logPart

// HttpContext -> Async<HttpContext option>
startWebServer defaultConfig app
