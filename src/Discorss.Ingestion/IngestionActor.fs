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

    let processMessage (inbox: MailboxProcessor<ActorMessage>) (state: ActorState<unit>) =
        task {
            let! msg = inbox.Receive()

            let! state =
                match msg with
                | ActorMessage.Start ->
                    msg |> Actor.post feedActor
                    do cancellation.Cancel()
                    state |> Task.ofResult
                | ActorMessage.Stop rc ->
                    do cancellation.Cancel()
                    feedActor |> Actor.stop
                    rc.Reply()
                    { state with stopped = true } |> Task.ofResult
                | ActorMessage.IngestFeeds
                | ActorMessage.IngestFeed _ ->
                    msg |> Actor.post feedActor
                    state |> Task.ofResult
                | ActorMessage.FeedEntry e ->
                    log.LogTrace $"Received feedentry {e.uri}..."
                    e |> ActorMessage.FeedEntry |> (Actor.post docActor)
                    state |> Task.ofResult
                | ActorMessage.Document d ->
                    log.LogTrace $"Received document {d.uri}..."

                    task {
                        do! notificationWriter.SetAsync d
                        return state
                    }
                | ActorMessage.IndexDocument d ->
                    msg |> (Actor.post indexingActor)
                    state |> Task.ofResult
                | _ -> state |> Task.ofResult

            return state
        }

    let rec loop inbox (state: ActorState<unit>) =
        async {
            let! state = processMessage inbox state |> Async.AwaitTask

            return!
                match state.stopped with
                | true -> async { }
                | _ -> loop inbox state
        }

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox { stopped = false; state = () })

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

    interface IOrchestrationActor with
        member this.Start() = actor.Post ActorMessage.Start

        member this.Stop() =
            actor.PostAndReply(fun rc -> ActorMessage.Stop rc)
