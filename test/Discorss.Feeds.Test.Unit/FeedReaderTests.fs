namespace Discorss.Feeds.Test.Unit

open System
open System.Xml.Linq
open Discorss.Feeds
open FsCheck
open FsCheck.Xunit
open FsUnit

module FeedReaderTests=
    
    [<Xunit.Theory>]
    [<Xunit.InlineData("MsRss20Feed.xml")>]    
    [<Xunit.InlineData("Rss20Feed.xml")>]
    let ``parse Rss 20``(name)=
        let feed = name |> TestHelpers.sampleFeedAsString |> FeedReader.parse "http://"

        feed |> should not' (equal None)

    [<Xunit.Theory>]
    [<Xunit.InlineData("Rss091Feed.xml")>]    
    let ``parse Rss 091``(name)=
        let feed = name |> TestHelpers.sampleFeedAsString |> FeedReader.parse "http://"

        feed |> should equal None

    [<Xunit.Theory>]
    [<Xunit.InlineData("Rss092Feed.xml")>]    
    let ``parse Rss 092``(name)=
        let feed = name |> TestHelpers.sampleFeedAsString |> FeedReader.parse "http://"

        feed |> should equal None

