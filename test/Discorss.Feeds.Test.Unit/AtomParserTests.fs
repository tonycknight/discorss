namespace Discorss.Feeds.Test.Unit

open System
open System.Xml.Linq
open Discorss.Feeds
open FsCheck
open FsCheck.Xunit
open FsUnit

module AtomParserTests =
        
    let atomDoc() = 
        "MsAtomFeed.xml" |> TestHelpers.sampleFeedAsString |> XDocument.Parse
        

    [<Xunit.Fact>]
    let ``isAtom empty XML doc``() =
        let doc = new XDocument()

        let r = AtomParser.isAtom doc
        
        r |> should equal false

    [<Xunit.Fact>]
    let ``isAtom sample Atom doc``() =
        let doc = atomDoc()

        let r = AtomParser.isAtom doc
        
        r |> should equal true

    [<Xunit.Fact>]
    let ``parse sample Atom doc``()=
        let doc = atomDoc()
        let url = "http://test.org"

        let feed = AtomParser.parse url doc |> Option.get
        
        feed.url |> should equal url
        feed.title |> should equal ".NET Blog"
        feed.description |> should equal "Free. Cross-platform. Open source. A developer platform for building all your apps."
        feed.entries |> should haveLength 10

