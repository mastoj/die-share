#load "references.fsx"
#load "app.fsx"

open Suave
open App

startWebServer defaultConfig app
