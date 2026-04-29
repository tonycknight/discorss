namespace Discorss.Ingestion

open System
open System.Diagnostics.CodeAnalysis
open Discorss
open Discorss.Feeds


type private FeedActorState = { feed: FeedInfo; lastRead: DateTime;  }

[<ExcludeFromCodeCoverage>]
type FeedActor(parent: IActor, feed: FeedInfo, feedProvider: IFeedProvider) as self =

    
    let getFeedDocuments () =
        task {
            let! r = feedProvider.GetFeedAsync feed.uri
            return
                match r with
                | FeedReadResult.Feed feed ->
                     // TODO: feed.entries |> toDocs |> ActorMessage.Documents |> Some
                     None
                | FeedReadResult.Error msg -> None
                | FeedReadResult.Xml xml -> None
        }


    let rec loop (inbox: MailboxProcessor<ActorMessage>) =
        task {

            match! inbox.Receive() with
            | ActorMessage.FetchFeed uri when feed.uri = uri ->
                let! m = getFeedDocuments ()
                m |> Option.iter parent.Post
            | ActorMessage.GetActorStats rc ->
                inbox |> Actor.getStats $"{self.GetType()} - {feed.uri}" |> rc.Reply
            | m -> parent.Post m

            return! loop inbox
        }

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox |> Async.AwaitTask) // TODO:

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.GetStats() =
            actor.PostAndAsyncReply(fun rc -> ActorMessage.GetActorStats rc)

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
