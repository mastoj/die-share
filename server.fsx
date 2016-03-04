#load "references.fsx"
#load "app.fs"

open Suave
open App

startWebServer defaultConfig app
