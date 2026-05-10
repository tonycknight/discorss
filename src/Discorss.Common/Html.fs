namespace Discorss

module Html =

    let stripHtml (html: string) =
        try
            let doc = new HtmlAgilityPack.HtmlDocument()
            doc.LoadHtml(html)
            doc.DocumentNode.InnerText |> Some
        with ex ->
            None
