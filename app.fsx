#load "references.fsx"

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

module Views =
    open Suave.Html
    module Index =
        type X = Attribute        
        let render content =
            html [
                head [
                    title "Die-Share"
                    linkAttr [
                        "src","something"
                    ]
                ]
                body [
                    div [
                        span (text "Hello")
                        content
                    ]
                ]
            ] |> xmlToString

open Suave.Html
let login =
    let result = Views.Index.render (span (text " world"))
    path "/login" >=> OK (result.ToString())

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
