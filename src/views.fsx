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
                            let link = sprintf "/%s" (x.ToLower())
                            yield liAttr ["class","pure-menu-item"] [(aAttr link ["class","pure-menu-link"] (text x))]
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
            cssLink "/content/css/site.css"

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

[<AutoOpen>]
module FormHelpers =
    let inputElem label inputEl =
        divClass ["pure-u-1"] [label; inputEl]

module AuthenticationView =
    let index returnUrl =
        let postUrl = sprintf "/login?returnurl=%s" returnUrl
        renderPage <|
            [
                formAttr ["class", "pure-form pure-form-stacked";"method","post";"action",postUrl;"enctype","multipart/x-www-form-urlencoded"]
                    [
                        fieldset
                            [
                                divClass ["pure-g"]
                                    [
                                        (inputElem
                                        // Description
                                            (labelAttr ["for","UserName"] (text "Name"))
                                            (inputAttr ["name", "UserName"; "type", "text";"class","pure-input-1-4"]))

                                        (inputElem
                                            (labelAttr ["for","Password"] (text "Password"))
                                            (div [inputAttr ["type","password";"name","Password";"class","pure-input-1-4"]]))
                                    ]

                                buttonAttr ["type","submit";"class","pure-button pure-button-primary"] (text "Logon")
                            ]
                    ]
            ]

module ExpenseReportView =
    let getProjects = ["Rock"; "Paper"; "Scissor"; "Lizard"; "Spock"]

    let expenseListItem expenseReport =
        let className =
            if expenseReport.Status <> ExpenseReportStatus.Created then " submitted" else ""
        liAttr ["class", "expense-report-item" + className]
            [
                a (sprintf "/expense/%i" expenseReport.Id)
                    [
                        spanAttr ["class", "expense-report-item-title"] (text expenseReport.Description)
                        spanAttr ["class", "expense-report-item-amount"] (text (sprintf " - %i kr" (expenseReport.Expenses |> List.sumBy (fun x -> x.Amount))))
                    ]
            ]

    let expenseList expenseReports =
        ulAttr ["class", "expense-report-list"] (expenseReports |> List.map expenseListItem)

    let expenses expenseReports =
        renderPage <|
            [
                h1 (text "Expenses")
                formAttr ["method","post";"action","/expense"] [buttonAttr ["type","submit";"class","pure-button pure-button-primary"] (text "New expense report")]
                expenseList expenseReports
            ]

    let handlebar templateName node =
        scriptAttr ["id",templateName; "type","text/x-handlebars-template"] [node]

    let expenseFormTemplate expenseReport =
        let submitUrl = sprintf "/api/expense/%i/submit" (expenseReport.Id)
        formAttr ["id","expense-form"; "class", "expense-form pure-form pure-form-stacked";"method","post";"action",submitUrl;"enctype","multipart/form-data"] [
            fieldset [
                divClass ["pure-g"] [
                    (inputElem
                        // Project
                        (labelAttr ["for","project"] (text "Project"))
                        (selectAttr ["name", "Project";"class","pure-input-1-4"]
                            ([
                                (text "{{#select Project}}")
                                (getProjects |> List.map (fun x -> option (text x)))
                                (text "{{/select}}")
                             ] |> List.concat)))

                    (inputElem
                    // Description
                        (labelAttr ["for","description"] (text "Description"))
                        (inputAttr ["name", "Description"; "type", "text";"class","pure-input-1-4";"value","{{Description}}"]))

                    ulAttr ["class","file-list pure-u-1"] [
                        (Text """{{#each Expenses}}""")
                        li [
                            spanAttr ["class", "file-name"] (text "File: {{this.File.FileName}}")
                            spanAttr ["class", "file-amout-container"] [
                                labelAttr ["for", "file_{{this.File.FileId}}"] (text "Amount: ")
                                inputAttr ["class", "file-amount"; "type", "text"; "value", "{{this.Amount}}"; "name", "file_{{this.File.FileId}}"; "id", "file_{{this.File.FileId}}"]
                            ]
                        ]
                        (Text """{{/each}}""")
                    ]
                    divClass ["expense-total"; "pure-u-1"] (text "Total: {{Total}} kr")
                    (Text "{{#if notSubmitted}}")
                    (inputElem
                        (labelAttr ["for","file"] (text "Files"))
                        (div [inputAttr ["type","file";"name","File";"class","pure-input-1-4 file-uploader"]]))
                    (Text "{{/if}}")
                ]

                (Text "{{#if notSubmitted}}")
                buttonAttr ["type","submit";"class","pure-button pure-button-primary"] (text "Submit expense")
                (Text "{{/if}}")
                (Text "{{#unless notSubmitted}}")
                spanAttr ["class", "expense-status"] (text "Submitted for approval")
                (Text "{{/unless}}")
            ]
        ]

    let details expenseReport =
        renderPage <|
            [
                jsLink "/content/js/app.js"
                divClass ["header-container"]
                    [h1 (text "File new expense")]
                divAttr ["id","expense-form-container"] []
                handlebar "expense-form-template" <| expenseFormTemplate expenseReport
            ]
