namespace Discorss.Feeds

open System
open System.Xml.Linq
open Discorss

module AtomParser =
    // Example: https://github.com/anthropics/claude-code/releases.atom

    let parseChannel (xml: XDocument) =
        match xml |> Xml.docElement "feed" with
        | Some channel ->
            let title =
                channel |> Xml.elementValueDefault "title" |> Html.stripHtml |> Strings.trim

            let description =
                channel
                |> Xml.elementValueDefault "description"
                |> Html.stripHtml
                |> Strings.trim

            (title, description)
        | _ -> ("", "")

    let parseEntries (xml: XDocument) =
        let uri (e: XElement) =
            match e |> Xml.elementValueDefault "link" with
            | "" ->
                e
                |> Xml.element "link"
                |> Option.bind (Xml.attributeValue "href")
                |> Option.defaultValue ""
            | x -> x

        let parse (e: XElement) =
            { FeedEntry.id = e |> Xml.elementValueDefault "link"
              publication = DateTime.UtcNow
              uri = uri e
              title = e |> Xml.elementValueDefault "title"
              description = e |> Xml.elementValueDefault "summary"
              author = e |> Xml.elementValueDefault "creator"
              content = e |> Xml.elementValueDefault "content"
              categories = e |> Xml.elementValues "category" |> Array.ofSeq }

        xml |> Xml.docElements "entry" |> Seq.map parse |> List.ofSeq

    let isMatch (xml: XDocument) =
        match Rss.rssVersion xml with
        | Some "atom" -> true
        | _ -> false

    let parse url (xml: XDocument) =
        match parseChannel xml with
        | ("", "") -> Choice2Of2 "Empty channel in feed"
        | (title, description) ->
            { Feed.uri = url
              feedType = FeedType.Atom
              title = title
              description = description
              updated = DateTime.UtcNow
              entries = parseEntries xml }
            |> Choice1Of2

    let (|IsAtom|_|) (xml: XDocument) =
        match xml |> isMatch with
        | true -> Some true
        | _ -> None
