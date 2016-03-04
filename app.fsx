#load "references.fsx"

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Cookie
open Suave.Html

module Views =
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

let tryLogin queryParam =
    let userName =
        match queryParam "userName" with
        | Choice1Of2 s -> s
        | _ -> failwith "no user name"
    let password =
        match queryParam "password" with
        | Choice1Of2 s -> s
        | _ -> failwith "no password"
    if userName = password
    then
        Authentication.authenticated Session false
        Redirection.FOUND "/"
    else Redirection.FOUND "/login"

let login successWebPart =
    let result = Views.Index.render (span (text " world"))
    choose [
        path "/login" >=> OK (result.ToString())
        path "/dologin" >=>
            request(fun r -> tryLogin r.queryParam)
        Authentication.authenticateWithLogin Session "/login" successWebPart
    ]

let start : WebPart =
    (fun ctx ->
        (path "/" >=> OK ("Hello start " + (ctx.userState.[Authentication.UserNameKey].ToString()))) ctx
        )

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
        content
        login <| choose [
            start
            expense
            api
        ]
    ]
