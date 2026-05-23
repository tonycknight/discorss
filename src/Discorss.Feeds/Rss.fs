namespace Discorss.Feeds

open System
open System.Xml.Linq
open Discorss

module Rss =
    let rssVersion (xml: XDocument) =
        match xml |> Xml.docElement "rss" with
        | Some rss -> rss |> Xml.attributeValue "version"
        | None ->
            match Xml.docName xml with
            | "rdf" -> Some "rdf"
            | "feed" -> Some "atom"
            | _ -> None
