namespace Discorss.Feeds.Test.Unit

open System
open System.Xml.Linq
open Discorss.Feeds
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit

module Rss092ParserTests =

    let sampleDoc () =
        "Rss092Feed.xml" |> TestHelpers.sampleFeedAsString |> XDocument.Parse


    [<Xunit.Fact>]
    let ``isMatch empty XML doc`` () =
        let doc = new XDocument()

        let r = Rss092Parser.isMatch doc

        r |> should equal false

    [<Xunit.Fact>]
    let ``isMatch sample feed doc`` () =
        let doc = sampleDoc ()

        let r = Rss092Parser.isMatch doc

        r |> should equal true

    [<Xunit.Fact>]
    let ``parse sample doc`` () =
        let doc = sampleDoc ()
        let url = "http://test.org"

        let feed = Rss092Parser.parse url doc |> Option.get

        feed.uri |> should equal url
        feed.title |> should equal "Dave Winer: Grateful Dead"

        feed.description
        |> should
            equal
            "A high-fidelity Grateful Dead song every day. This is where we're experimenting with enclosures on RSS news items that download when you're not using your computer. If it works (it will) it will be the end of the Click-And-Wait multimedia experience on the Internet. "

        feed.entries |> should haveLength 17

        feed.entries
        |> Seq.forall (fun e -> e.author |> String.IsNullOrWhiteSpace |> not)
        |> should equal true

        feed.entries
        |> Seq.forall (fun e -> e.description |> String.IsNullOrWhiteSpace |> not)
        |> should equal true
