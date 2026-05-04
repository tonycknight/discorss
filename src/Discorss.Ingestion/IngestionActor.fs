namespace Discorss.Ingestion

open System
open System.Diagnostics.CodeAnalysis
open Discorss
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

[<ExcludeFromCodeCoverage>]
type IngestionActor
    (
        logFactory: ILoggerFactory,
        config: IOptions<AppConfiguration>,
        feedActor: FeedIngestionActor,
        docActor: DocumentIngestionActor
    ) as self =

    let log = logFactory.CreateLogger<IngestionActor>()
    let cancellation = new System.Threading.CancellationTokenSource()

    let postIngestTimer =
        (fun args -> ActorMessage.IngestFeeds |> Actor.post self)
        |> Actor.createTimer config.Value.feedIngestionFrequency

    let rec loop (inbox: MailboxProcessor<ActorMessage>) =
        task {
            let! msg = inbox.Receive()

            match msg with
            | ActorMessage.Start -> do postIngestTimer.Enabled <- true
            | ActorMessage.Stop ->
                do postIngestTimer.Enabled <- false
                do cancellation.Cancel()
            | ActorMessage.IngestFeeds
            | ActorMessage.IngestFeed _ -> msg |> Actor.post feedActor
            | ActorMessage.FeedEntry e ->
                log.LogTrace $"Received feedentry {e.uri}..."
                e |> ActorMessage.FeedEntry |> (Actor.post docActor)
            | ActorMessage.Document d ->
                log.LogTrace $"Received document {d.uri}..."
                ignore 0 // TODO:
            | _ -> ignore 0

            return! loop inbox
        }

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox |> Async.AwaitTask)

    member this.QueueNames =
        [ Discorss.Queues.QueueNames.feedEntries; Discorss.Queues.QueueNames.documents ]

    interface IActor with
        member this.GetStats() =
            actor |> Actor.getStats (self.GetType().Name)

        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
