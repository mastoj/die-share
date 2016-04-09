#load "references.fsx"
#load "expense.fsx"
#load "helpers.fsx"
open Expense
open Suave.Successful
open Suave.Json
open Suave.Http
open Suave.RequestErrors
open Helpers

module Expense =
    let getExpenseReport expenseReportService expenseId _ =
        expenseReportService.GetExpenseReport expenseId
        |> (function
            | Some er ->
                er
                |> toJson
                |> OK
            | None ->
                NOT_FOUND "Not found")

    let submitExpenseReport expenseReportService id =
        expenseReportService.SubmitExpenseReport id
        getExpenseReport expenseReportService id

    let updateExpense expenseReportService request =
        request
        |> fromJson<ExpenseReport>
        |> expenseReportService.UpdateExpenseReport
        OK ""

    let getExpenseReports expenseReportService _ =
        expenseReportService.GetExpenseReports "tomas"
        |> toJson
        |> OK

    let uploadFile expenseId (request:Suave.Http.HttpRequest) =
        request.multiPartFields |> List.iter (printfn "Multipart fields: %A")
        let file = request.files |> List.head
        let fileId = FileIO.saveFile expenseId file.fileName file.tempFilePath
        {
            FileName = file.fileName
            FileId = fileId
            MimeType = file.mimeType
        }
        |> toJson
        |> OK
