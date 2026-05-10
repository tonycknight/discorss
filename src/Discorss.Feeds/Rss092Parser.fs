namespace Discorss.Feeds

open System
open System.Xml.Linq
open Discorss

module Rss092Parser =

    let parseChannel (xml: XDocument) =
        match xml |> Xml.docElement "channel" with
        | Some channel ->
            let title = channel |> Xml.elementValueDefault "title" |> Html.stripHtml
            let description = channel |> Xml.elementValueDefault "description" |> Html.stripHtml
            (title, description)
        | _ -> ("", "")

    let parseEntries (xml: XDocument) =
        let parse (e: XElement) =
            { FeedEntry.id = e |> Xml.elementValueDefault "link"
              publication = DateTime.UtcNow
              uri = e |> Xml.elementValueDefault "link"
              title = e |> Xml.elementValueDefault "title"
              description = e |> Xml.elementValueDefault "description"
              author = e |> Xml.elementValueDefault "creator"
              content = e |> Xml.elementValueDefault "encoded"
              categories = e |> Xml.elementValues "category" |> Array.ofSeq }

        xml |> Xml.docElements "item" |> Seq.map parse |> List.ofSeq

    let isMatch (xml: XDocument) =
        match Rss.rssVersion xml with
        | Some "0.92" -> true
        | _ -> false

    let parse url (xml: XDocument) =

        let title, description = parseChannel xml

        let result =
            { Feed.uri = url
              feedType = FeedType.Rss092
              title = title
              description = description
              updated = DateTime.UtcNow
              entries = parseEntries xml }

        result |> Some

    let (|IsRss092|_|) (xml: XDocument) =
        match xml |> isMatch with
        | true -> Some true
        | _ -> None
