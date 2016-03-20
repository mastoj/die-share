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
    let saveFile httpFile =
        System.IO.Directory.CreateDirectory(__SOURCE_DIRECTORY__ + "/uploads/") |> ignore
        System.IO.File.Move(httpFile.tempFilePath, __SOURCE_DIRECTORY__ + "/uploads/" + httpFile.fileName)

    choose [
        path "/expenses" >=>
            choose [
                GET >=> OK (Expense.newExpense())
                POST >=> (
                    fun x ->
                        printfn "Request: %A" x
                        printfn "Files: %A" x.request.files
                        printfn "Formdata: %A" (x.request.formData "description")
                        printfn "Files2: %A" (x.request.formData "file[0]")
                        x.request.multiPartFields |> List.iter (printfn "Multipart fields: %A")
                        x.request.files |> List.iter (fun x -> x |> saveFile)
                        OK "Posted" x)
            ]
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
