namespace Discorss

module Html =

    let innerHtml (html: string) =
        try
            let doc = new HtmlAgilityPack.HtmlDocument()
            doc.LoadHtml(html)
            doc.DocumentNode.InnerText |> Some
        with ex ->
            None

    let stripHtml (value: string) =
        let value = value |> Option.ofNull |> Option.defaultValue ""

        value |> innerHtml |> Option.defaultValue value |> Strings.trim
