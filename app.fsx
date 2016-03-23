#load "references.fsx"
#load "views.fsx"
#load "expense.fsx"

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

open Expense
open Views

[<AutoOpen>]
module WebModels =
    type FileUploadResult = {
        FileId: int
        FileName: string
        MimeType: string
    }

[<AutoOpen>]
module Helpers =
    let toJson value = JsonConvert.SerializeObject(value)
    let fromJson<'T> (request:HttpRequest) =
        let json = System.Text.Encoding.UTF8.GetString(request.rawForm)
        JsonConvert.DeserializeObject<'T>(json)

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

module Api =
    let getExpenseReport expenseService expenseId _ =
        expenseService.GetExpenseReport expenseId
        |> toJson
        |> OK

    let submitExpenseReport expenseService id _ =
        expenseService.SubmitExpenseReport id
        OK ""

    let updateExpense expenseService request =
        request
        |> fromJson<ExpenseReport>
        |> expenseService.UpdateExpenseReport
        OK ""

    let getExpenseReports expenseService _ =
        expenseService.GetExpenseReports "tomas"
        |> toJson
        |> OK

    let uploadFile expenseId request =
        request.multiPartFields |> List.iter (printfn "Multipart fields: %A")
        let file = request.files |> List.head
        let fileId = file |> saveFile expenseId
        let result =
            {
                FileName = file.fileName
                FileId = fileId
                MimeType = file.mimeType
            }
        let resultString = JsonConvert.SerializeObject(result)
        OK resultString

    let expenseApi expenseService : WebPart =
        choose [
            path "/expenses" >=> GET >=> request(getExpenseReports expenseService)
            pathScan "/api/expense/%i" (fun expenseId ->
                choose [
                    GET >=> request(getExpenseReport expenseService expenseId)
                    PUT >=> request(updateExpense expenseService)
                ])
            pathScan "/api/expense/%i/submit" (fun expenseId ->
                    POST >=> request(submitExpenseReport expenseService expenseId)
                )
            pathScan "/api/expense/%i/file" (fun expenseId ->
                choose [
                    POST >=> request(uploadFile expenseId)
                ])
        ]

    let part expenseService =
        Writers.setMimeType "application/json" >=>
            choose [
                expenseApi expenseService
            ]

module Web =
    let expense expenseService =
        choose [
            path "/expenses" >=>
                choose [
                    GET >=> OK (Expense.newExpense())
                ]
            path "/expense"
                >=> POST
                >=> request(fun _ ->
                    let er = expenseService.CreateExpenseReport "tomas"
                    Redirection.redirect (sprintf "/expense/%i" er.Id))
            pathScan "/expense/%i" (fun i ->
                    choose [
                        GET >=> request(fun _ -> expenseService.GetExpenseReport i |> toJson |> OK)
                        POST >=> OK "HELL"
                    ])
        ]

    let part expenseService =
        Writers.setMimeType "text/html" >=>
            choose [
                expense expenseService
                path "/" >=> (OK (Index()))
            ]

module Content =
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

    let part =
        choose [
            pathScan "/content/%s.%s" readContent
        ]

let app =
    let expenseService = createExpenseService()
    choose [
        Content.part
        Web.part expenseService
        Api.part expenseService
    ]
