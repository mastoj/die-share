#r "./packages/Suave/lib/net40/Suave.dll"

open Suave
open Suave.Successful
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors

// HttpContext -> Async<HttpContext option>

module Logging =
    let log reqId prefix msg (ctx:HttpContext) =
        printfn "==> %A %s: %s" reqId prefix (msg())
        succeed ctx

    let logTime reqId prefix =
        log reqId prefix (fun () -> System.DateTime.Now.ToString("O"))

    let logUrl reqId (ctx:HttpContext) =
        log reqId "Url" (fun () -> ctx.request.url.AbsolutePath) ctx

    let logStartTime reqId = logTime reqId "Start request"
    let logEndTime reqId = logTime reqId "End request"

    let logRequest part =
        context(fun _ ->
            let reqId = System.Guid.NewGuid()
            (logStartTime reqId) >=> logUrl reqId >=> part >=> (logEndTime reqId)
        )

let hello1 ctx =
    async {
        let responseBytes = System.Text.Encoding.UTF8.GetBytes("Hello world")
        let response = {
                ctx.response with content = Bytes responseBytes
            }
        return (Some {ctx with response = response})
    }
let hello2 = OK "Hello world!"
let hello3 (msg, i) = OK (sprintf "Hello %s %i!" msg i)

let app =
    choose [
        path "/" >=> hello1
        path "/hello2" >=> hello2
        pathScan "/hello/%s/%i" hello3
        NOT_FOUND "#wat"
    ] |> Logging.logRequest

//startWebServer defaultConfig app
