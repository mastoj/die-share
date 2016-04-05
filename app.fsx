#load "references.fsx"
#load "expense.fsx"
#load "views.fsx"
#load "authentication.fsx"

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
open Authentication

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

[<AutoOpen>]
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

[<AutoOpen>]
module Api =
    let getExpenseReport expenseReportService expenseId _ =
        expenseReportService.GetExpenseReport expenseId
        |> (function
            | Some er ->
                er
                |> toJson
                |> OK
            | None ->
                RequestErrors.NOT_FOUND "Not found")

    let submitExpenseReport expenseReportService id _ =
        expenseReportService.SubmitExpenseReport id
        OK ""

    let updateExpense expenseReportService request =
        request
        |> fromJson<ExpenseReport>
        |> expenseReportService.UpdateExpenseReport
        OK ""

    let getExpenseReports expenseReportService _ =
        expenseReportService.GetExpenseReports "tomas"
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

[<AutoOpen>]
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

let app =
    let expenseReportService = createExpenseReportService()
    choose [
        pathScan "/content/%s.%s" readContent

        choose [
            path "/" >=> (OK (Home.index()))
            path "/login" >=>
                choose [
                    GET >=> request(fun r -> OK (AuthenticationView.index (Authentication.getReturnUrl r)))
                    POST >=> loginUser
                ]
            path "/logout" >=> logout >=> (Redirection.redirect "/")
            Authentication.protect <|
                choose [
                    path "/expenses" >=>
                        choose [
                            GET >=> context(fun c ->
                                let userName = getUserName c |> Option.get
                                let expenseReports = expenseReportService.GetExpenseReports userName
                                OK (ExpenseReportView.expenses expenseReports))
                        ]
                    path "/expense"
                        >=> POST
                        >=> request(fun _ ->
                            let er = expenseReportService.CreateExpenseReport "tomas"
                            Redirection.redirect (sprintf "/expense/%i" er.Id))
                    pathScan "/expense/%i" (fun i ->
                            choose [
                                GET >=> context(fun c ->
                                            expenseReportService.GetExpenseReport i
                                            |> (function
                                                    | Some er ->
                                                        er
                                                        |> ExpenseReportView.details
                                                        |> OK
                                                    | None -> Suave.RequestErrors.NOT_FOUND "No matching expense report"))
                                POST >=> OK "HELL"
                            ])
                ]
        ] >=> Writers.setMimeType "text/html"

        protect <|
            choose [
                path "/expenses" >=> GET >=> request(getExpenseReports expenseReportService)
                pathScan "/api/expense/%i" (fun expenseId ->
                    choose [
                        GET >=> request(getExpenseReport expenseReportService expenseId)
                        PUT >=> request(updateExpense expenseReportService)
                    ])
                pathScan "/api/expense/%i/submit" (fun expenseId ->
                        POST >=> request(submitExpenseReport expenseReportService expenseId)
                    )
                pathScan "/api/expense/%i/file" (fun expenseId ->
                    choose [
                        POST >=> request(uploadFile expenseId)
                    ])
            ] >=> Writers.setMimeType "application/json"
    ]
