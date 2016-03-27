#load "references.fsx"
open System

type Agent<'T> = MailboxProcessor<'T>

type File = {
    FileId: int
    FileName: string
    MimeType: string
}

type Expense = {
    File: File
    Amount: int
}

type ExpenseReportStatus =
    | Created = 0
    | Submitted = 1
    | Approved = 2

type ExpenseReport =
    {
        Id: int
        User: string
        Project: string
        Description: string
        Expenses: Expense list
        Status: ExpenseReportStatus
    } with
    static member create id user =
        {
            Id = id
            User = user
            Project = ""
            Description = ""
            Expenses = []
            Status = ExpenseReportStatus.Created
        }

let getTotal expenseReport =
    expenseReport.Expenses |> List.sumBy (fun x -> x.Amount)

type Messages =
    | CreateExpenseReport of string * AsyncReplyChannel<ExpenseReport>
    | UpdateExpenseReport of ExpenseReport
    | SubmitExpenseReport of int
    | GetExpenseReports of string * AsyncReplyChannel<ExpenseReport list>
    | GetExpenseReport of int * AsyncReplyChannel<ExpenseReport option>

type ExpenseService = {
    CreateExpenseReport: string -> ExpenseReport
    UpdateExpenseReport: ExpenseReport -> unit
    SubmitExpenseReport: int -> unit
    GetExpenseReports: string -> ExpenseReport list
    GetExpenseReport: int -> ExpenseReport option
}

let createExpenseReportService() =
    let random = new Random()
    let agent = Agent.Start(fun inbox ->
        let rec loop state =
            async {
                let! message = inbox.Receive()
                printfn "Got message: %A" message
                match message with
                | CreateExpenseReport (user, reply) ->
                    let id = random.Next(99999)
                    let expenseReport = ExpenseReport.create id user
                    reply.Reply(expenseReport)
                    return! loop (state |> Map.add id expenseReport)
                | UpdateExpenseReport er ->
                    return! loop (state |> Map.add er.Id er)
                | SubmitExpenseReport id ->
                    let er = state |> Map.find id
                    return! loop (state |> Map.add er.Id {er with Status = ExpenseReportStatus.Submitted})
                | GetExpenseReports (user, reply) ->
                    let ers =
                        state
                        |> Map.filter (fun k v -> v.User = user)
                        |> Map.toList
                        |> List.map snd
                    reply.Reply(ers)
                    return! loop state
                | GetExpenseReport (id, reply) ->
                    let er = state |> Map.tryFind id
                    reply.Reply(er)
                    return! loop state
            }
        loop Map.empty
        )
    {
        CreateExpenseReport = (fun user -> agent.PostAndReply(fun r -> CreateExpenseReport(user,r)))
        UpdateExpenseReport = (fun er -> agent.Post(UpdateExpenseReport er))
        SubmitExpenseReport = (fun id -> agent.Post(SubmitExpenseReport id))
        GetExpenseReports = (fun user -> agent.PostAndReply(fun r -> GetExpenseReports(user,r)))
        GetExpenseReport = (fun id -> agent.PostAndReply(fun r -> GetExpenseReport(id,r)))
    }
