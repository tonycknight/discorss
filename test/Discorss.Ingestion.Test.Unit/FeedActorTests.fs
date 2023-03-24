namespace Discorss.Ingestion.Tests.Unit

open System
open System.Threading.Tasks
open Discorss
open Discorss.Ingestion
open FsUnit
open NSubstitute
open Xunit

module FeedActorTests =

    [<Fact>]
    let ``Post parent receives Feeds`` () =
        let parent = Substitute.For<IActor>()
        let config = AppConfiguration.defaultConfig
        let http = Substitute.For<IInternalHttpClient>()

        http
            .GetAsync(Arg.Any<string>())
            .Returns(
                HttpRequestResponse.HttpOkRequestResponse(Net.HttpStatusCode.OK, "[]")
                |> Task.fromResult
            )
        |> ignore

        let feedUri = "test"

        let actor = new FeedActor(parent, config, http, feedUri) :> IActor
        ActorMessage.FetchFeed feedUri |> actor.Post

        Task.Delay(1000).GetAwaiter().GetResult() // TODO: fix
        parent.Received(1).Post(Arg.Any<ActorMessage>())
