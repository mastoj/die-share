#load "references.fsx"

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.State
open Suave.Html

let script2Attr attr = tag "script" attr empty

let jsLink src = script2Attr ["src", src]
let cssLink href = linkAttr [ "href", href; " rel", "stylesheet"; " type", "text/css" ]
let divClass classes = divAttr ["class", (classes |> String.concat " ") ]

let ulAttr = tag "ul"
let ul = ulAttr [ ]

let liAttr = tag "li"
let li = liAttr [ ]

let h1Attr = tag "h1"
let h1 = h1Attr [ ]

let h2Attr = tag "h2"
let h2 = h2Attr [ ]


let formAttr = tag "form"
let form = formAttr []

let fieldsetAttr = tag "fieldset"
let fieldset = fieldsetAttr []

let legendAttr = tag "legend"
let legend = legendAttr []

let labelAttr = tag "label"
let label = labelAttr []

let selectAttr = tag "select"
let select = selectAttr []

let optionAttr = tag "option"
let option = optionAttr []

let buttonAttr = tag "button"
let button = buttonAttr []
