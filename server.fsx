#r "./packages/Suave/lib/net40/Suave.dll"
open Suave
open Suave.Successful
open Suave.Operators
open Suave.Filters
open Suave.RequestErrors

module Logging =
    let log prefix msg (ctx:HttpContext) =
        printfn "==> %s: %s" prefix (msg())
        succeed ctx

    let logTime prefix =
        log prefix (fun () -> System.DateTime.Now.ToString("O"))

    let logUrl (ctx:HttpContext) =
        log "Url" (fun () -> ctx.request.url.AbsolutePath) ctx

    let logStartTime = logTime "Start request"
    let logEndTime = logTime "End request"

    let logRequest part =
        logStartTime >=> logUrl >=> part >=> logEndTime

let hello1 (ctx:HttpContext) =
    async {
        System.Threading.Thread.Sleep(5000)
        let responseBytes = System.Text.Encoding.UTF8.GetBytes("Hello world")
        let response : HttpResult = {
                ctx.response with content = Bytes responseBytes
            }
        return (Some {ctx with response = response})
    }

let hello2 = OK "Hello world 2"

let app =
    choose [
        path "/" >=> hello1
        path "/hello2" >=> hello2
        NOT_FOUND "#wat"
    ] |> Logging.logRequest

//startWebServer defaultConfig app
