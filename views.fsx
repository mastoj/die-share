#load "references.fsx"
#load "expense.fsx"
#load "html.fsx"

open Suave.Html

open Expense
open Html

[<AutoOpen>]
module Layout =
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

    let render authHeader content =
        let authScript =
            match authHeader with
            | None -> ""
            | Some basicToken -> sprintf "(function() { window.DS = window.DS || {}; DS.auth = '%s';})();" basicToken

        html [
          head [

            script (text authScript)

            title "Die-Share"
            metaAttr ["charset","utf-8"]
            metaAttr ["name","viewport"; "content","width=device-width, initial-scale=1.0"]
            cssLink "http://yui.yahooapis.com/pure/0.6.0/pure-min.css"
            cssLink "http://yui.yahooapis.com/pure/0.6.0/grids-responsive-min.css"
            cssLink "/content/css/site.css"

            jsLink "http://builds.handlebarsjs.com.s3.amazonaws.com/handlebars.runtime-v4.0.5.js"
          ]
          body [
            headerMenu
            content
          ]
        ] |> renderHtmlDocument

    let renderPage authHeader page =
        render authHeader <|
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
        render None <|
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

module ExpenseReportView =
    let inputElem label inputEl =
        divClass ["pure-u-1"] [label; inputEl]

    let getProjects = ["Rock"; "Paper"; "Scissor"; "Lizard"; "Spock"]

    let expenseListItem expenseReport =
        liAttr ["class", "expense-report-item"]
            [
                a (sprintf "/expense/%i" expenseReport.Id)
                    [
                        spanAttr ["class", "expense-report-item-title"] (text expenseReport.Description)
                        spanAttr ["class", "expense-report-item-amount"] (text (sprintf " - %i" (expenseReport.Expenses |> List.sumBy (fun x -> x.Amount))))
                    ]
            ]

    let expenseList expenseReports =
        ulAttr ["class", "expense-report-list"] (expenseReports |> List.map expenseListItem)

    let expenses authHeader expenseReports =
        renderPage authHeader <|
            [
                h1 (text "Expenses")
                formAttr ["method","post";"action","/expense"] [buttonAttr ["type","submit";"class","pure-button pure-button-primary"] (text "New expense report")]
                expenseList expenseReports
            ]

    let handlebar templateName node =
        scriptAttr ["id",templateName; "type","text/x-handlebars-template"] [node]

    let details authHeader expenseReport =
        renderPage authHeader <|
            [
                jsLink "/content/js/app.js"
                divClass ["header-container"]
                    [h1 (text "File new expense")]
                divAttr ["id","expense-form-container"] []
                handlebar "expense-form-template" <|
                    formAttr ["id","expense-form"; "class", "pure-form pure-form-stacked";"method","post";"action","/expenses";"enctype","multipart/form-data"]
                        [
                            fieldset
                                [
                                    divClass ["pure-g"]
                                        [
                                            (inputElem
                                                // Project
                                                (labelAttr ["for","project"] (text "Project"))
                                                (selectAttr ["name", "project";"class","pure-input-1-4"]
                                                    ([
                                                        (text "{{#select Project}}")
                                                        (getProjects |> List.map (fun x -> option (text x)))
                                                        (text "{{/select}}")
                                                     ] |> List.concat)))

                                            (inputElem
                                            // Description
                                                (labelAttr ["for","description"] (text "Description"))
                                                (inputAttr ["name", "description";"class","pure-input-1-4";"value","{{description}}"]))

                                            (inputElem
                                                (labelAttr ["for","file"] (text "Files"))
                                                (div [inputAttr ["type","file";"name","file";"class","pure-input-1-4 file-uploader"]]))
                                        ]

                                    buttonAttr ["type","submit";"class","pure-button pure-button-primary"] (text "Submit expense")
                                ]
                        ]
            ]
