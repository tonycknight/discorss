namespace Discorss.Feeds

open System
open System.Xml.Linq

module Rss=
    let rssVersion(xml: XDocument) = 
        match xml |> Xml.docElement "rss" with
        | Some rss -> rss |> Xml.attributeValue "version" 
        | None -> None
            

