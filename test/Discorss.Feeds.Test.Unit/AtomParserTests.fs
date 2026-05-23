namespace Discorss.Feeds.Test.Unit

open System
open System.Xml.Linq
open Discorss.Feeds
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit

module AtomParserTests =
    let sampleDoc () =
        "AtomFeed.xml" |> TestHelpers.sampleFeedAsString |> XDocument.Parse

    [<Xunit.Fact>]
    let ``isMatch empty XML doc`` () =
        let doc = new XDocument()

        let r = AtomParser.isMatch doc

        r |> should equal false

    [<Xunit.Fact>]
    let ``isMatch sample feed doc`` () =
        let doc = sampleDoc ()

        let r = AtomParser.isMatch doc

        r |> should equal true

    [<Xunit.Fact>]
    let ``parse sample doc`` () =
        let doc = sampleDoc ()
        let url = "http://test.org"

        match AtomParser.parse url doc with
        | Choice2Of2 e -> Exception(e) |> raise
        | Choice1Of2 feed ->
            feed.uri |> should equal url
            feed.feedType |> should equal FeedType.Atom
            feed.title |> should equal "Release notes from claude-code"
            feed.description |> should equal ""

            feed.entries |> should haveLength 10

            feed.entries
            |> Seq.forall (fun e -> e.title |> String.IsNullOrWhiteSpace |> not)
            |> should equal true

            feed.entries
            |> Seq.forall (fun e -> e.content |> String.IsNullOrWhiteSpace |> not)
            |> should equal true

            feed.entries
            |> Seq.forall (fun e -> e.author |> String.IsNullOrWhiteSpace)
            |> should equal true

            feed.entries
            |> Seq.forall (fun e -> e.uri |> String.IsNullOrWhiteSpace |> not)
            |> should equal true
