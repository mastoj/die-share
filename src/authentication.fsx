#load "references.fsx"
#load "path.fsx"

open Suave
open Suave.Cookie
open Suave.RequestErrors
open Suave.Authentication
open Suave.Utils
open Suave.Form
open Suave.Model.Binding

type Login = {
    UserName : string
    Password : Password
}

let login : Form<Login> = Form ([],[])

let inline private addUserName ctx username =
    { ctx with userState = ctx.userState |> Map.add UserNameKey (box username) }

let returnUrlQueryParam = "returnurl"
let getReturnUrl (req:HttpRequest) =
    match req.queryParam returnUrlQueryParam with
    | Choice1Of2 url ->
        printfn "Got %A in choice 1" (req.queryParam returnUrlQueryParam)
        url
    | Choice2Of2 _ ->
        printfn "Got %A in choice 2" (req.queryParam returnUrlQueryParam)
        Path.home

let handleLogin (req:HttpRequest) logonData =
    let (Password passwordText) = logonData.Password
    if logonData.UserName = passwordText
    then
        let redirectionUrl = getReturnUrl req
        authenticate CookieLife.Session false
          (fun() -> logonData.UserName |> ASCII.bytes |> Choice1Of2)
          (sprintf "%A" >> RequestErrors.BAD_REQUEST >> Choice2Of2)
          (Redirection.redirect redirectionUrl)
    else
        Redirection.redirect Path.login

let userLogin (req:HttpRequest) =
        bindReq (bindForm login) (handleLogin req) BAD_REQUEST

let logout =
    Suave.Cookie.unsetCookie SessionAuthCookie

let authenticateForms (logonPart:WebPart) (protectedPart:WebPart) =
    let continuation =
        context(fun ctx ->
            match ctx.response.cookies |> readCookies ctx.runtime.serverKey SessionAuthCookie with
            | Choice1Of2 (cookie, cookieValueBytes) ->
                cookieValueBytes
                |> ASCII.toString
                |> addUserName ctx
                |> (fun ctx' -> (fun _ -> protectedPart ctx'))
            | Choice2Of2 _ -> challenge)

    context(fun ctx ->
        authenticate CookieLife.Session false
              (fun() -> logonPart |> Choice2Of2)
              (sprintf "%A" >> RequestErrors.BAD_REQUEST >> Choice2Of2)
              continuation
        )

let protect protectedPart =
    request(fun r ->
            let returnUrl = r.url.AbsolutePath
            let loginUrl = Path.login + sprintf "?%s=%s" returnUrlQueryParam returnUrl
            authenticateForms (Redirection.redirect loginUrl) protectedPart
        )

let getUserName (c:HttpContext) =
    c.userState |> Map.tryFind "userName" |> Option.map (fun x -> x.ToString())
