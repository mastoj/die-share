#load "references.fsx"
#load "app.fsx"

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators
open App

let app2 = OK (sprintf "HELLO WORLD %A" System.DateTime.Now.Ticks)

// HttpContext -> Async<HttpContext option>

let webpart1 (ctx:HttpContext) =
    let hellobytes = System.Text.Encoding.UTF8.GetBytes(sprintf "Hello world %A" System.DateTime.Now)
    async {
        return {
            ctx with
                response = { ctx.response with status = HTTP_200; content = Bytes hellobytes }
        } |> Some
    }

startWebServer defaultConfig app2
