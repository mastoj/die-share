#load "references.fsx"

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

let login =
    path "/login" >=> OK "Hello login"

let start =
    path "/" >=> OK "Hello start"

let api =
    choose [
        pathScan "/api/expense/%s" (fun _ -> OK "Hello api")
    ]

let expense =
    choose [
        path "/expense/new" >=> OK "New expense"
        pathScan "/expense/%i" (fun i -> OK (sprintf "New expense %A" i))
    ]

let content =
    pathScan "/content/%s" (fun _ -> OK "Hello content")

let app =
    choose [
        start
        login
        expense
        api
        content
    ]
