#load "references.fsx"
#load "path.fsx"
#load "content.fsx"
#load "views.fsx"

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Writers
open Suave.Operators

let app =
    choose [
        pathScan Path.Content.file Content.readContent
        choose [
            path Path.home >=> (OK (Views.Home.index()))
        ] >=> Writers.setMimeType "text/html"
    ]
