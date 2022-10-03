namespace Discorss

module Html=
    open HtmlAgilityPack

    let private loadHtml(html:string)=
        let doc = new HtmlDocument()
        doc.LoadHtml(html)
        doc

    let stripHtml (html:string)=
        try
            let doc = loadHtml html
            doc.DocumentNode.InnerText |> Some
        with
        | ex -> None
        
