#load "references.fsx"
#load "views.fsx"

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.State
open Suave.Html

let scrtip2Attr attr = tag "script" attr empty

let jsLink src = scrtip2Attr ["src", src]
let cssLink href = linkAttr [ "href", href; " rel", "stylesheet"; " type", "text/css" ]
let ulAttr = tag "ul"
let ul = pAttr [ ]
let liAttr = tag "li"
let li = pAttr [ ]
let h1Attr = tag "h1"
let h1 = pAttr [ ]
let h2Attr = tag "h2"
let h2 = pAttr [ ]

let headerMenu =
    divAttr ["class","pure-menu pure-menu-horizontal"]
        [
            aAttr "#" ["class", "pure-menu-heading"] (text "DIE-SHARE")
            ulAttr ["class","pure-menu-list"]
                [
                    for x in ["Expenses"; "Travels"; "Do not click this"] do
                        yield liAttr ["class","pure-menu-item"] [(aAttr "#" ["class","pure-menu-link"] (text x))]
                ]
        ]

let render content =
    html [
      head [
        title "Die-Share"
        metaAttr ["charset","utf-8"]
        metaAttr ["name","viewport"; "content","width=device-width, initial-scale=1.0"]
        cssLink "http://yui.yahooapis.com/pure/0.6.0/pure-min.css"
        cssLink "content/css/site.css"
        jsLink "content/js/app.js"
      ]
      body [
        headerMenu
//      <div class="banner">
//          <h1 class="banner-head">
//              Simple Pricing.<br>
//              Try before you buy.
//          </h1>
//      </div>
//

        content
      ]
    ] |> renderHtmlDocument

let Index() =
    render <|
        divAttr ["class","banner"]
            [h1Attr ["class","banner-head"] [Text "Reporting expenses"; br; Text "Should be simple!"]]
