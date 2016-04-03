#load "references.fsx"

open Suave
open Suave.Cookie
open Suave.RequestErrors
open Suave.Authentication
open Suave.Utils

let internal parseAuthenticationToken (token : string) =
  let parts = token.Split (' ')
  let enc = parts.[1].Trim()
  let decoded = ASCII.decodeBase64 enc
  let indexOfColon = decoded.IndexOf(':')
  (parts.[0].ToLower(), decoded.Substring(0,indexOfColon), decoded.Substring(indexOfColon+1))

let inline private addUserName username ctx = { ctx with userState = ctx.userState |> Map.add UserNameKey (box username) }

let internal validateAuthHeader validationFunction (ctx:HttpContext) =
  let req = ctx.request
  match req.header "authorization" with
    | Choice1Of2 header ->
      let (typ, username, password) = parseAuthenticationToken header
      if (typ.Equals("basic")) && validationFunction (username,password) then
        ASCII.bytes header |> Choice1Of2
      else
        challenge |> Choice2Of2
    | Choice2Of2 _ ->
      challenge |> Choice2Of2

let authenticateBasicWithCookie relativeExpiry secure validationFunction (protectedPart:WebPart) =
  let continuation =
    context(fun ctx ->
      match ctx.response.cookies |> readCookies ctx.runtime.serverKey SessionAuthCookie with
      | Choice1Of2 (cookie, cookieValueBytes) ->
        cookieValueBytes
        |> ASCII.toString
        |> parseAuthenticationToken
        |> (fun(_,userName,_) -> addUserName userName ctx)
        |> (fun ctx' -> (fun _ -> protectedPart ctx'))
      | Choice2Of2 _ -> challenge)

  context(fun ctx ->
    authenticate relativeExpiry secure
      (fun() -> validateAuthHeader validationFunction ctx)
      (sprintf "%A" >> RequestErrors.BAD_REQUEST >> Choice2Of2)
      continuation
    )
