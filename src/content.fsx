#load "references.fsx"
open System.IO
open Suave
open Suave.Successful
open Suave.Operators

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
