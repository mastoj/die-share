#load "references.fsx"
#load "views.fsx"

open Newtonsoft.Json
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.State
open Suave.Html
open System
open System.IO
open Views

[<AutoOpen>]
module WebModels =
    type FileUploadResult = {
        FileId: int
        FileName: string
        MimeType: string
    }

module FileIO =
    open System.IO
    let move src dest =
        if File.Exists(dest) then File.Delete(dest)
        File.Move(src, dest)

    let createDirectory path =
        if not (Directory.Exists(path)) then Directory.CreateDirectory(path) |> ignore

let random = new Random()
let saveFile expenseId httpFile =
    let fileId = random.Next(999999)
    let directoryPath = sprintf "%s/uploads/%i/%i/" __SOURCE_DIRECTORY__ expenseId fileId
    let targetFilePath = sprintf "%s%s" directoryPath httpFile.fileName
    FileIO.createDirectory (directoryPath)
    FileIO.move httpFile.tempFilePath (targetFilePath)
    fileId

let expenseApi : WebPart =
    choose [
        pathScan "/api/expense/%i" (fun expenseId ->
            choose [
                PUT >=> OK "Expense updated"
            ])
        pathScan "/api/expense/%i/file" (fun expenseId ->
            choose [
                POST >=>
                    request(
                        fun x ->
                            x.multiPartFields |> List.iter (printfn "Multipart fields: %A")
                            let file = x.files |> List.head
                            let fileId = file |> saveFile expenseId
                            let result =
                                {
                                    FileName = file.fileName
                                    FileId = fileId
                                    MimeType = file.mimeType
                                }
                            let resultString = JsonConvert.SerializeObject(result)
                            (OK resultString >=> Writers.setMimeType "application/json"))
            ])
    ]

let api =
    Writers.setMimeType "application/json" >=>
        choose [
            expenseApi
        ]

let expense =
    choose [
        path "/expenses" >=>
            choose [
                GET >=> OK (Expense.newExpense())
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
