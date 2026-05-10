namespace Discorss

module Html =

    let innerHtml (html: string) =
        try
            let doc = new HtmlAgilityPack.HtmlDocument()
            doc.LoadHtml(html)
            doc.DocumentNode.InnerText |> Some
        with ex ->
            None

    let decode (value: string) =
        System.Web.HttpUtility.HtmlDecode value

    let stripHtml (value: string) =
        let value = value |> Option.ofNull |> Option.defaultValue ""

        value |> innerHtml |> Option.defaultValue value |> decode |> Strings.trim
