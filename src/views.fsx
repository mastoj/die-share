#load "references.fsx"
#load "html.fsx"
#load "path.fsx"

open Suave.Html

open Html

[<AutoOpen>]
module Layout =
    let headerMenu =
        divClass ["pure-menu"; "pure-menu-horizontal"]
            [
                aAttr "#" ["class", "pure-menu-heading"] (text "MY-SHARE")
                ulAttr ["class","pure-menu-list"]
                    [
                        for (header, path) in [("Expenses",Path.Expense.index); ("Travels", Path.home); ("Do not click this", Path.logout)] do
                            yield liAttr ["class","pure-menu-item"] [(aAttr path ["class","pure-menu-link"] (text header))]
                    ]
            ]

    let render content =
        html [
          head [
            title "My-Share"
            metaAttr ["charset","utf-8"]
            metaAttr ["name","viewport"; "content","width=device-width, initial-scale=1.0"]
            cssLink "http://yui.yahooapis.com/pure/0.6.0/pure-min.css"
            cssLink "http://yui.yahooapis.com/pure/0.6.0/grids-responsive-min.css"
            cssLink (sprintf Path.Content.CSS.file "site.css")

            jsLink "https://code.jquery.com/jquery-2.2.2.min.js"
            jsLink "http://builds.handlebarsjs.com.s3.amazonaws.com/handlebars-v4.0.5.js"
          ]
          body [
            headerMenu
            content
          ]
        ] |> renderHtmlDocument

    let renderPage page =
        render <|
            divClass ["layout"] page

module Home =
    let reportTile (reportType:string) columns =
            divClass ["pure-u-1"; (sprintf "pure-u-md-1-%i" columns); "tile"]
                [
                    a (sprintf "%s" (reportType.ToLower()))
                        [
                            divClass ["tile-content"]
                                (text reportType)
                        ]
                ]

    let index() =
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
                                    for x in ["Expenses"; "Travels"] do
                                        yield reportTile x 2
                                ]
                        ]
                ]
