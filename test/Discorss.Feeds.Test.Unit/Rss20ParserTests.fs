namespace Discorss.Feeds.Test.Unit

open System
open System.Xml.Linq
open Discorss.Feeds
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit

module Rss20ParserTests =

    let sampleDoc () =
        "MsRss20Feed.xml" |> TestHelpers.sampleFeedAsString |> XDocument.Parse


    [<Xunit.Fact>]
    let ``isMatch empty XML doc`` () =
        let doc = new XDocument()

        let r = Rss20Parser.isMatch doc

        r |> should equal false

    [<Xunit.Fact>]
    let ``isMatch sample feed doc`` () =
        let doc = sampleDoc ()

        let r = Rss20Parser.isMatch doc

        r |> should equal true

    [<Xunit.Fact>]
    let ``parse sample doc`` () =
        let doc = sampleDoc ()
        let url = "http://test.org"

        let feed = Rss20Parser.parse url doc |> Option.get

        feed.uri |> should equal url
        feed.title |> should equal ".NET Blog"

        feed.description
        |> should equal "Free. Cross-platform. Open source. A developer platform for building all your apps."

        feed.entries |> should haveLength 10

        feed.entries
        |> Seq.forall (fun e -> e.title |> String.IsNullOrWhiteSpace |> not)
        |> should equal true

        feed.entries
        |> Seq.forall (fun e -> e.description |> String.IsNullOrWhiteSpace |> not)
        |> should equal true

        feed.entries
        |> Seq.forall (fun e -> e.author |> String.IsNullOrWhiteSpace |> not)
        |> should equal true

        feed.entries
        |> Seq.forall (fun e -> e.uri |> String.IsNullOrWhiteSpace |> not)
        |> should equal true
