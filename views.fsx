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
let divClass classes = divAttr ["class", (classes |> String.concat " ") ]

let headerMenu =
    divClass ["pure-menu"; "pure-menu-horizontal"]
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
        cssLink "http://yui.yahooapis.com/pure/0.6.0/grids-responsive-min.css"
        cssLink "content/css/site.css"
        jsLink "content/js/app.js"
      ]
      body [
        headerMenu
        content
      ]
    ] |> renderHtmlDocument

let reportTile reportType columns =
    divClass ["pure-u-1"; (sprintf "pure-u-md-1-%i" columns)]
        [
            divClass ["pricing-table";"pricing-table-free"]
                [
                    h2 (text reportType)
                    aAttr (sprintf "/%s" (reportType.ToLower())) ["class", "button-choose pure-button"] (text "Choose")
                ]
        ]

let Index() =
    render <|
        div
            [
                divClass ["banner"]
                    [
                        h1Attr ["class","banner-head"] [Text "Reporting expenses"; br; Text "Should be simple!"]
                    ]
                divClass ["l-content"]
                    [
                        divClass ["pricing-tables";"pure-g"]
                            [
                                for x in ["Expenses"; "Travel"] do
                                    yield reportTile x 2
                            ]
                    ]
            ]
//
//
//            <div class="pure-u-1 pure-u-md-1-3">
//                <div class="pricing-table pricing-table-free">
//                    <div class="pricing-table-header">
//                        <h2>Personal</h2>
//
//                        <span class="pricing-table-price">
//                            $5 <span>per month</span>
//                        </span>
//                    </div>
//
//                    <ul class="pricing-table-list">
//                        <li>Free setup</li>
//                        <li>Custom sub-domain</li>
//                        <li>Standard customer support</li>
//                        <li>1GB file storage</li>
//                        <li>1 database</li>
//                        <li>Unlimited bandwidth</li>
//                    </ul>
//
//                    <button class="button-choose pure-button">Choose</button>
//                </div>
//            </div>
