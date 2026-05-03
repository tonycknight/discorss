namespace Discorss.Feeds

open System
open System.Xml.Linq
open Discorss

module Rss =
    let rssVersion (xml: XDocument) =
        match xml |> Xml.docElement "rss" with
        | Some rss -> rss |> Xml.attributeValue "version"
        | None -> None

    let dehtmlify (value: string) =
        value |> Discorss.Html.stripHtml |> Option.defaultValue value
