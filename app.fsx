#load "references.fsx"
#load "views.fsx"

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.State
open Suave.Html
open System.IO
open Views

let api =
    Writers.setMimeType "application/json" >=>
        choose [
            pathScan "/api/expense/%s" (fun _ -> OK "Hello api")
        ]

let expense =
    choose [
        path "/expense/new" >=> OK "New expense"
        pathScan "/expense/%i" (fun i -> OK (sprintf "New expense %A" i))
    ]

type ContentType =
    | JS
    | CSS

let parseContentType = function
    | "js" -> Some JS
    | "css" -> Some CSS
    | _ -> None

let file str = File.ReadAllText(__SOURCE_DIRECTORY__ + "/content/" + str)

let readContent (path,fileEnding) =
    let fileContent = file (sprintf "%s.%s" path fileEnding)
    request(fun _ ->
        let contentType = fileEnding |> parseContentType
        let mimetype =
            match contentType with
            | Some JS -> "application/javascript"
            | Some CSS -> "text/css"
            | None -> raise (exn "Not supported content type")
        Writers.setMimeType mimetype
        >=> OK (file (path + "." + fileEnding))
    )

let web =
    Writers.setMimeType "text/html" >=>
        choose [
            expense
            path "/" >=> (OK (Index()))
        ]

let content =
    choose [
        pathScan "/content/%s.%s" readContent
    ]

let app =
    choose [
        web
        content
        api
    ]
