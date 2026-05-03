namespace Discorss.Feeds.Test.Unit

open System
open System.Xml.Linq
open Discorss.Feeds
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit

module RdfParserTests =
    let sampleDoc () =
        "RdfFeed.xml" |> TestHelpers.sampleFeedAsString |> XDocument.Parse

    [<Xunit.Fact>]
    let ``isMatch empty XML doc`` () =
        let doc = new XDocument()

        let r = RdfParser.isMatch doc

        r |> should equal false

    [<Xunit.Fact>]
    let ``isMatch sample feed doc`` () =
        let doc = sampleDoc ()

        let r = RdfParser.isMatch doc

        r |> should equal true

    [<Xunit.Fact>]
    let ``parse sample doc`` () =
        let doc = sampleDoc ()
        let url = "http://test.org"

        let feed = RdfParser.parse url doc |> Option.get

        feed.uri |> should equal url
        feed.title |> should equal "Slashdot"

        feed.description |> should equal "News for nerds, stuff that matters"

        feed.entries |> should haveLength 15
        feed.entries |> Seq.forall (fun e -> e.title |> String.IsNullOrWhiteSpace |> not) |> should equal true
        feed.entries |> Seq.forall (fun e -> e.description |> String.IsNullOrWhiteSpace |> not) |> should equal true
        feed.entries |> Seq.forall (fun e -> e.author |> String.IsNullOrWhiteSpace |> not) |> should equal true
        feed.entries |> Seq.forall (fun e -> e.uri |> String.IsNullOrWhiteSpace |> not) |> should equal true