namespace Discorss.Ingestion

open System
open System.Diagnostics.CodeAnalysis
open Microbroker.Client
open Microsoft.Extensions.Logging

[<ExcludeFromCodeCoverage>]
type IngestionActor(logFactory: ILoggerFactory, broker: IMicrobrokerProxy, feedActor: FeedIngestionActor, docActor: DocumentIngestionActor) as self =

    let log = logFactory.CreateLogger<IngestionActor>()
    let cancellation = new System.Threading.CancellationTokenSource()

    let postIngestTimer =
        (fun args -> ActorMessage.IngestFeeds |> Actor.post self)
        |> Actor.createTimer (TimeSpan.FromSeconds 15.)

    let getStats inbox =
        Actor.getStats (self.GetType().Name) inbox

    let rec loop (inbox: MailboxProcessor<ActorMessage>) =
        task {
            let! msg = inbox.Receive()

            match msg with
            | ActorMessage.Start -> do postIngestTimer.Enabled <- true
            | ActorMessage.Stop ->
                do postIngestTimer.Enabled <- false
                do cancellation.Cancel()
            | ActorMessage.IngestFeeds
            | ActorMessage.AddFeed _
            | ActorMessage.RemoveFeed _
            | ActorMessage.IngestFeed _ -> msg |> Actor.post feedActor
            | ActorMessage.FeedEntry e ->
                log.LogTrace $"Received feedentry {e.uri}..."
                e |> Models.toDocument |> ActorMessage.Document |> (Actor.post docActor)                
            | ActorMessage.GetActorStats rc -> inbox |> getStats |> rc.Reply
            // TODO: need to pull messages from microbroker queues
            | _ -> ignore 0

            return! loop inbox
        }

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox |> Async.AwaitTask)

    member this.QueueNames = [ Discorss.Queues.QueueNames.feedEntries ]

    interface IActor with
        member this.GetStats() =
            actor.PostAndAsyncReply(fun rc -> ActorMessage.GetActorStats rc)

        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
