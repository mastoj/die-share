open System
type IntPath = PrintfFormat<(int -> string),unit,string,string,int>
type IntIntPath = PrintfFormat<(int -> int -> string),unit,string,string,(int*int)>
type StringPath = PrintfFormat<(string -> string),unit,string,string,string>
type StringStringPath = PrintfFormat<(string -> string -> string),unit,string,string,(string*string)>

let home = "/"
let login = "/login"
let logout = "/logout"

module Expense =
    let index = "/expenses"
    let create = "/expense"
    let details:IntPath = "/expense/%i"

module Api =
    module Expense =
        let details:IntPath = "/api/expense/%i"
        let submit:IntPath = "/api/expense/%i/submit"
        let fileUpload:IntPath = "/api/expense/%i/file"

module Content =
    let file:StringStringPath = "/content/%s.%s"

    module JS =
        let file:StringPath = "/content/js/%s"

    module CSS =
        let file:StringPath = "/content/css/%s"

module Server =
    let RootFolder = __SOURCE_DIRECTORY__ + "/../"

    module File =
        let uploadFolder:IntIntPath = "uploads/%i/%i/"
