#load "references.fsx"
#load "path.fsx"
#load "helpers.fsx"
#load "expense.fsx"
#load "api.fsx"
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
open Helpers
open Api

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
            path Path.home >=> (OK (Home.index()))
            path Path.login >=>
                choose [
                    GET >=> request(fun r -> OK (AuthenticationView.index (Authentication.getReturnUrl r)))
                    POST >=> userLogin
                ]
            path Path.logout >=> logout >=> (Redirection.redirect Path.home)
            Authentication.protect <|
                choose [
                    path Path.Expense.index >=>
                        choose [
                            GET >=> context(fun c ->
                                let userName = getUserName c |> Option.get
                                let expenseReports = expenseReportService.GetExpenseReports userName
                                OK (ExpenseReportView.expenses expenseReports))
                        ]
                    path Path.Expense.create
                        >=> POST
                        >=> context(fun c ->
                            let er = expenseReportService.CreateExpenseReport (getUserName c |> Option.get)
                            Redirection.redirect (sprintf Path.Expense.details er.Id))
                    pathScan Path.Expense.details (fun i ->
                            choose [
                                GET >=> context(fun c ->
                                            expenseReportService.GetExpenseReport i
                                            |> (function
                                                    | Some er ->
                                                        er
                                                        |> ExpenseReportView.details
                                                        |> OK
                                                    | None -> Suave.RequestErrors.NOT_FOUND "No matching expense report"))
                            ])
                ]
        ] >=> Writers.setMimeType "text/html"

        protect <|
            choose [
                pathScan Path.Api.Expense.details (fun expenseId ->
                    choose [
                        GET >=> request(getExpenseReport expenseReportService expenseId)
                        PUT >=> request(updateExpense expenseReportService)
                    ])
                pathScan Path.Api.Expense.submit (fun expenseId ->
                        POST >=> request(submitExpenseReport expenseReportService expenseId)
                    )
                pathScan Path.Api.Expense.fileUpload (fun expenseId ->
                    choose [
                        POST >=> request(uploadFile expenseId)
                    ])
            ] >=> Writers.setMimeType "application/json"
    ]
