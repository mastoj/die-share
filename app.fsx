#load "references.fsx"
#load "views.fsx"

open Newtonsoft.Json
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.State
open Suave.Html
open System.IO
open Views

module FileIO =
    open System.IO
    let move src dest =
        if File.Exists(dest) then File.Delete(dest)
        File.Move(src, dest)

    let createDirectory path =
        if not (Directory.Exists(path)) then Directory.CreateDirectory(path) |> ignore

let api =
    Writers.setMimeType "application/json" >=>
        choose [
            pathScan "/api/expense/%s" (fun _ -> OK "Hello api")
        ]

type FileUploadResult = {
    FileId: int
    FileName: string
}
let expense =
    let saveFile httpFile =
        FileIO.createDirectory (__SOURCE_DIRECTORY__ + "/uploads/")
        FileIO.move httpFile.tempFilePath (__SOURCE_DIRECTORY__ + "/uploads/" + httpFile.fileName)

    choose [
        path "/expenses" >=>
            choose [
                GET >=> OK (Expense.newExpense())
                POST >=> request(
                    fun x ->
                        printfn "Request: %A" x
                        printfn "Raw data: %A" x.rawForm
                        printfn "Formdata: %A" (x.formData "description")
                        printfn "Files2: %A" (x.formData "file[0]")
                        x.multiPartFields |> List.iter (printfn "Multipart fields: %A")
                        x.files |> List.iter (fun x -> x |> saveFile)
                        let result =
                            {
                                FileName = "Hello.pdf"
                                FileId = 34
                            }
                        let resultString = JsonConvert.SerializeObject(result)
                        (OK resultString >=> Writers.setMimeType "application/json"))
            ]
        pathScan "/expense/%i" (fun i -> OK (sprintf "New expense %A" i))
    ]

type ContentType =
    | JS
    | CSS

let parseContentType = function
    | "js" -> Some JS
    | "css" -> Some CSS
    | _ -> None

let file str = File.ReadAllText(__SOURCE_DIRECTORY__ + "/content/" + str)

let readContent (path,fileEnding) =
    let fileContent = file (sprintf "%s.%s" path fileEnding)
    request(fun _ ->
        let contentType = fileEnding |> parseContentType
        let mimetype =
            match contentType with
            | Some JS -> "application/javascript"
            | Some CSS -> "text/css"
            | None -> raise (exn "Not supported content type")
        Writers.setMimeType mimetype
        >=> OK (file (path + "." + fileEnding))
    )

let web =
    Writers.setMimeType "text/html" >=>
        choose [
            expense
            path "/" >=> (OK (Index()))
        ]

let content =
    choose [
        pathScan "/content/%s.%s" readContent
    ]

let app =
    choose [
        web
        content
        api
    ]
