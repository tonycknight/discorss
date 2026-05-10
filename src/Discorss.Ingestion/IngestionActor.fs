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
        docActor: DocumentIngestionActor,
        notificationWriter: Documents.IDocumentNotificationWriter,
        broker: Microbroker.Client.IMicrobrokerProxy
    ) as self =

    let log = logFactory.CreateLogger<IngestionActor>()
    let cancellation = new System.Threading.CancellationTokenSource()

    let postIngestTimer =
        (fun args -> ActorMessage.IngestFeeds |> Actor.post self)
        |> Actor.createTimer config.Value.feedIngestionFrequency

    let queueStats () =
        task {
            let! queueCounts = self.QueueNames |> Array.ofSeq |> broker.GetQueueCountsAsync

            return
                queueCounts
                |> Seq.map (fun qc ->
                    { Stats.name = qc.name
                      itemCount = qc.count
                      childStats = [] })
                |> List.ofSeq
        }

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
                do! notificationWriter.SetAsync d

            | _ -> ignore 0

            return! loop inbox
        }

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox |> Async.AwaitTask)

    member this.QueueNames =
        [ Discorss.Queues.QueueNames.feedEntries; Discorss.Queues.QueueNames.documents ]

    interface IStatsSource with
        member this.GetStatsAsync() =
            task {
                let stats = actor |> Actor.getStats (self.GetType().Name)
                let! queueStats = queueStats ()

                return { stats with childStats = queueStats }
            }

    interface IActor with

        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
