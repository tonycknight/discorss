namespace Discorss.Feeds

open System
open System.Xml.Linq
open Discorss

module FeedReader =

    let private parseBodyToXml body =
        try
            XDocument.Parse(body) |> FeedReadResult.Xml
        with :? System.Xml.XmlException as ex ->
            FeedReadResult.Error ex.Message

    let private clean (feed: Feed) =
        let cleanEntry (entry: FeedEntry) =
            { entry with
                title = Html.stripHtml entry.title
                author = Html.stripHtml entry.author
                description = Html.stripHtml entry.description
                content = Html.stripHtml entry.content
                categories = entry.categories |> Array.map Html.stripHtml }

        { feed with
            title = feed.title |> Html.stripHtml
            description = feed.description |> Html.stripHtml
            entries = feed.entries |> List.map cleanEntry }

    let private parser (xml: XDocument) =
        match xml with
        | Rss20Parser.IsRss20 x -> Rss20Parser.parse |> Choice1Of2
        | Rss092Parser.IsRss092 x -> Rss092Parser.parse |> Choice1Of2
        | Rss091Parser.IsRss091 x -> Rss091Parser.parse |> Choice1Of2
        | RdfParser.IsRdf x -> RdfParser.parse |> Choice1Of2
        | AtomParser.IsAtom x -> AtomParser.parse |> Choice1Of2
        | _ -> Choice2Of2 "Unrecognised feed type"

    let private parseXmlToFeed (url: string) (xml: XDocument) =
        match parser xml with
        | Choice2Of2 e -> FeedReadResult.Error e
        | Choice1Of2 parse ->
            match parse url xml with
            | Choice1Of2 feed -> feed |> clean |> FeedReadResult.Feed
            | Choice2Of2 e -> FeedReadResult.Error e

    let parse url body =
        parseBodyToXml body
        |> (function
        | FeedReadResult.Xml xml -> parseXmlToFeed url xml
        | x -> x)

    let readAsync (client: IExternalHttpClient) url =
        task {
            let! resp = client.GetAsync url

            return
                match resp with
                | HttpOkRequestResponse(status, body) -> body |> parse url
                | HttpErrorRequestResponse(status, body) -> FeedReadResult.Error(sprintf "HTTP %A received" status)
                | HttpExceptionRequestResponse(ex) -> FeedReadResult.Error ex.Message
        }
