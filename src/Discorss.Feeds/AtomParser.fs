namespace Discorss.Feeds

open System
open System.Xml.Linq
open Discorss

module AtomParser = 
    // Example: https://github.com/anthropics/claude-code/releases.atom
    let isMatch (xml: XDocument) =
        match Rss.rssVersion xml with
        | Some "atom" -> true
        | _ -> false

    let parse url (xml: XDocument) =
        Choice2Of2 "TODO: Atom not supported"

    let (|IsAtom|_|) (xml: XDocument) =
        match xml |> isMatch with
        | true -> Some true
        | _ -> None
