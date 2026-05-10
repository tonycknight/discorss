namespace Discorss.Feeds.Test.Unit

open System
open System.Net
open System.Threading.Tasks
open Discorss
open Discorss.Feeds
open FsCheck
open FsUnit.CustomMatchers
open FsUnit.Xunit
open NSubstitute

type Fact = Xunit.FactAttribute
type Theory = Xunit.TheoryAttribute
type InlineData = Xunit.InlineDataAttribute

module FeedReaderTests =

    let private mockHttpClient reqResp =

        let client = Substitute.For<IExternalHttpClient>()
        client.GetAsync(Arg.Any<string>()).Returns(Task.FromResult(reqResp)) |> ignore

        client

    [<Theory>]
    [<InlineData("MsRss20Feed.xml")>]
    [<InlineData("Rss20Feed.xml")>]
    let ``parse Rss 20`` (name) =
        let feed = name |> TestHelpers.sampleFeedAsString |> FeedReader.parse "http://"

        feed |> should be (ofCase <@ FeedReadResult.Feed @>)

    [<Theory>]
    [<InlineData("Rss091Feed.xml")>]
    let ``parse Rss 091`` (name) =
        let feed = name |> TestHelpers.sampleFeedAsString |> FeedReader.parse "http://"

        feed |> should be (ofCase <@ FeedReadResult.Feed @>)

    [<Theory>]
    [<InlineData("Rss092Feed.xml")>]
    let ``parse Rss 092`` (name) =
        let feed = name |> TestHelpers.sampleFeedAsString |> FeedReader.parse "http://"

        feed |> should be (ofCase <@ FeedReadResult.Feed @>)

    [<Theory>]
    [<InlineData("RdfFeed.xml")>]
    let ``parse Rdf`` (name) =
        let feed = name |> TestHelpers.sampleFeedAsString |> FeedReader.parse "http://"

        feed |> should be (ofCase <@ FeedReadResult.Feed @>)



    [<Theory>]
    [<InlineData("")>]
    [<InlineData("aa")>]
    [<InlineData("<xml>")>]
    [<InlineData("<xml/>")>]
    let ``read receives OK with malformed body`` (body) =

        let client =
            HttpRequestResponse.HttpOkRequestResponse(HttpStatusCode.OK, body)
            |> mockHttpClient

        let read = "url" |> FeedReader.readAsync client

        let result = read.Result

        result |> should be (ofCase <@ FeedReadResult.Error @>)

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("aa")>]
    [<InlineData("<xml>")>]
    [<InlineData("<xml/>")>]
    let ``read receives error with malformed body`` (body) =

        let fact =
            HttpRequestResponse.HttpErrorRequestResponse(HttpStatusCode.InternalServerError, body)
            |> mockHttpClient

        let read = "url" |> FeedReader.readAsync fact

        let result = read.Result

        result |> should be (ofCase <@ FeedReadResult.Error @>)

    [<Fact>]
    let ``read receives exception with malformed body`` () =

        let fact =
            HttpRequestResponse.HttpExceptionRequestResponse(new Exception())
            |> mockHttpClient

        let read = "url" |> FeedReader.readAsync fact

        let result = read.Result

        result |> should be (ofCase <@ FeedReadResult.Error @>)
