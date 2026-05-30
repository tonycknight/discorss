namespace Discorss.Ingestion

open System.Diagnostics.CodeAnalysis
open Discorss
open Microsoft.Extensions.Logging

[<ExcludeFromCodeCoverage>]
type IngestionActor
    (
        logFactory: ILoggerFactory,
        feedActor: FeedIngestionActor,
        docActor: DocumentIngestionActor,
        indexingActor: DocumentIndexingActor,
        notificationWriter: Documents.IDocumentNotificationWriter,
        broker: Microbroker.Client.IMicrobrokerProxy
    ) as self =

    let log = logFactory.CreateLogger<IngestionActor>()
    let cancellation = new System.Threading.CancellationTokenSource()

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

    let processMessage (inbox: MailboxProcessor<ActorMessage>) =
        task {
            let! msg = inbox.Receive()

            match msg with
            | ActorMessage.Start ->
                msg |> Actor.post feedActor
                do cancellation.Cancel()
            | ActorMessage.Stop rc ->                
                do cancellation.Cancel()
                feedActor |> Actor.stop
                // TODO: prevent further actions
                rc.Reply ()
            | ActorMessage.IngestFeeds
            | ActorMessage.IngestFeed _ -> msg |> Actor.post feedActor
            | ActorMessage.FeedEntry e ->
                log.LogTrace $"Received feedentry {e.uri}..."
                e |> ActorMessage.FeedEntry |> (Actor.post docActor)
            | ActorMessage.Document d ->
                log.LogTrace $"Received document {d.uri}..."
                do! notificationWriter.SetAsync d
            | ActorMessage.IndexDocument d -> msg |> (Actor.post indexingActor)

            | _ -> ignore 0
        }

    let rec loop inbox =
        async {
            do! processMessage inbox |> Async.AwaitTask

            return! loop inbox
        }

    let actor = MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox)

    member this.QueueNames =
        [ Discorss.Queues.QueueNames.feedEntries
          Discorss.Queues.QueueNames.documents
          Queues.QueueNames.documentIndexing ]

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
        member this.Stop() = actor.PostAndReply(fun rc -> ActorMessage.Stop rc)