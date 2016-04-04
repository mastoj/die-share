#load "references.fsx"

open Suave
open Suave.Cookie
open Suave.RequestErrors
open Suave.Authentication
open Suave.Utils
open Suave.Form
open Suave.Model.Binding

type Logon = {
    UserName : string
    Password : Password
}

let logon : Form<Logon> = Form ([],[])

let inline private addUserName ctx username =
    { ctx with userState = ctx.userState |> Map.add UserNameKey (box username) }

let handleLogin logonData =
    let (Password passwordText) = logonData.Password
    if logonData.UserName = passwordText
    then
        authenticate CookieLife.Session false
          (fun() -> logonData.UserName |> ASCII.bytes |> Choice1Of2)
          (sprintf "%A" >> RequestErrors.BAD_REQUEST >> Choice2Of2)
          (Redirection.redirect "/")
    else Redirection.redirect "/login"

let logonUser =
    bindReq (bindForm logon) handleLogin BAD_REQUEST

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
