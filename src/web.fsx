#load "references.fsx"
#load "views.fsx"
#load "authentication.fsx"
#load "expense.fsx"

open Suave
open Suave.Successful

open Authentication
open Expense

open Views

let index = OK (Views.Home.index())

module Login =
    let index (req:HttpRequest) =
        OK (AuthenticationView.index (Authentication.getReturnUrl req))

module Expense =
    let index expenseReportService (ctx:HttpContext) =
        let userName = getUserName ctx |> Option.get
        let expenseReports = expenseReportService.GetExpenseReports userName
        OK (ExpenseReportView.expenses expenseReports)

    let create expenseReportService (ctx:HttpContext) =
        let er = expenseReportService.CreateExpenseReport (getUserName ctx |> Option.get)
        Redirection.redirect (sprintf Path.Expense.details er.Id)

    let details expenseReportService id (ctx:HttpContext) =
        expenseReportService.GetExpenseReport id
        |> (function
                | Some er ->
                    er
                    |> ExpenseReportView.details
                    |> OK
                | None -> Suave.RequestErrors.NOT_FOUND "No matching expense report")
