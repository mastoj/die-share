#load "references.fsx"
#load "path.fsx"

[<AutoOpen>]
module Json =
    open Newtonsoft.Json
    open Suave.Http
    let toJson value = JsonConvert.SerializeObject(value)
    let fromJson<'T> (request:HttpRequest) =
        let json = System.Text.Encoding.UTF8.GetString(request.rawForm)
        JsonConvert.DeserializeObject<'T>(json)

module FileIO =
    open System
    open System.IO

    let move src dest =
        if File.Exists(dest) then File.Delete(dest)
        File.Move(src, dest)

    let createDirectory path =
        if not (Directory.Exists(path)) then Directory.CreateDirectory(path) |> ignore

    let random = new Random()
    let saveFile expenseId fileName tmpFilePath =
        let fileId = random.Next(999999)
        let directoryPath = sprintf Path.Server.File.uploadFolder expenseId fileId
        let targetFilePath = sprintf "%s%s" directoryPath fileName
        createDirectory directoryPath
        move tmpFilePath targetFilePath
        fileId
