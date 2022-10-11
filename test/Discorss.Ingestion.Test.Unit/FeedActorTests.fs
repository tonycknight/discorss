namespace Discorss.Ingestion.Tests.Unit

open System
open System.Threading.Tasks
open Discorss
open Discorss.Ingestion
open FsUnit
open NSubstitute
open Xunit

module FeedActorTests=

    [<Fact>]
    let ``Post TODO``()=
        let parent = Substitute.For<IActor>()
        let config = AppConfiguration.defaultConfig
        let http = Substitute.For<IInternalHttpClient>()
        http.GetAsync(Arg.Any<string>()).Returns(HttpRequestResponse.HttpOkRequestResponse(Net.HttpStatusCode.OK, "[]") |> Task.fromResult) |> ignore
        let feedUri = "test"

        let actor = new FeedActor(parent, config, http, feedUri)  :> IActor
        //actor.Start()
        ActorMessage.QueryFeed feedUri |> actor.Post
        // TODO: result??? there's no promise to hang on to
        Task.Delay(10000).GetAwaiter().GetResult()
        ignore 0
        // parent should have a Documents message posted to it ... eventually