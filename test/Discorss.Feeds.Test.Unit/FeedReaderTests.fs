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
type Property = FsCheck.Xunit.PropertyAttribute

module FeedReaderTests =

    let private mockHttpClient reqResp =

        let client = Substitute.For<IExternalHttpClient>()
        client.GetAsync(Arg.Any<string>()).Returns(Task.FromResult(reqResp)) |> ignore

        client

    [<Theory>]
    [<InlineData("MsRss20Feed.xml")>]
    [<InlineData("Rss20Feed.xml")>]
    [<InlineData("Rss091Feed.xml")>]
    [<InlineData("Rss092Feed.xml")>]
    [<InlineData("RdfFeed.xml")>]
    let ``parse feed returns feed`` (name) =
        let feed = name |> TestHelpers.sampleFeedAsString |> FeedReader.parse "http://"

        feed |> should be (ofCase <@ FeedReadResult.Feed @>)

    [<Theory>]
    [<InlineData("MsRss20Feed.xml")>]
    [<InlineData("Rss20Feed.xml")>]
    [<InlineData("Rss091Feed.xml")>]
    [<InlineData("Rss092Feed.xml")>]
    [<InlineData("RdfFeed.xml")>]
    let ``parse feed with stripped HTML`` (name) =
        let notContainHtml (value: string) =
            value.IndexOf('<') < 0 && value.IndexOf('>') < 0

        let notEmpty (value: string) = value.Length > 0

        match name |> TestHelpers.sampleFeedAsString |> FeedReader.parse "http://" with
        | FeedReadResult.Feed feed ->
            feed.description |> notContainHtml |> should equal true
            feed.title |> notContainHtml |> should equal true

            feed.entries
            |> List.iter (fun e ->
                e.title |> notContainHtml |> should equal true
                e.description |> notEmpty |> should equal true
                e.author |> notContainHtml |> should equal true
                e.categories |> Array.iter (notContainHtml >> should equal true)
                e.categories |> Array.iter (notEmpty >> should equal true))
        | x -> new Exception($"{x} returned") |> raise

    [<Property>]
    let ``parse random strings returns error`` (body: NonEmptyString) =
        match body.Get |> FeedReader.parse "http://" with
        | FeedReadResult.Error e -> true
        | _ -> false

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("aa")>]
    [<InlineData("<xml>")>]
    [<InlineData("<xml/>")>]
    let ``read returns OK with malformed body`` (body) =

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
    let ``read returns error with malformed body`` (body) =

        let fact =
            HttpRequestResponse.HttpErrorRequestResponse(HttpStatusCode.InternalServerError, body)
            |> mockHttpClient

        let read = "url" |> FeedReader.readAsync fact

        let result = read.Result

        result |> should be (ofCase <@ FeedReadResult.Error @>)

    [<Fact>]
    let ``read returns exception with malformed body`` () =

        let fact =
            HttpRequestResponse.HttpExceptionRequestResponse(new Exception())
            |> mockHttpClient

        let read = "url" |> FeedReader.readAsync fact

        let result = read.Result

        result |> should be (ofCase <@ FeedReadResult.Error @>)
