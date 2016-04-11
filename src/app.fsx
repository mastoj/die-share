#load "references.fsx"
#load "path.fsx"
#load "content.fsx"

open Suave
open Suave.Filters

let app =
    choose [
        pathScan Path.Content.file Content.readContent
    ]
