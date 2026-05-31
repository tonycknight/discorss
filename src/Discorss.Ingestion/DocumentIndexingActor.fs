namespace Discorss.Ingestion

open System.Diagnostics.CodeAnalysis
open Discorss
open Microbroker.Client
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

[<ExcludeFromCodeCoverage>]
type DocumentIndexingActor
    (
        logFactory: ILoggerFactory,
        config: IOptions<AppConfiguration>,
        docAnalyser: Documents.IDocumentAnalyser,
        statsRepo: Documents.IDocumentStatisticsRepository,
        broker: IMicrobrokerProxy
    ) as self =

    let log = logFactory.CreateLogger<DocumentIndexingActor>()

    let indexDocument (doc: Documents.Document) =
        task {
            log.LogTrace $"Indexing document {doc.uri}..."

            try
                let stats = doc |> docAnalyser.GetStatistics

                let! _ = statsRepo.SetAsync stats
                log.LogInformation($"Statistics written for document {doc.uri}.")

            with ex ->
                log.LogError(ex, $"Error calculating statistics for document {doc.uri}")
                doc |> ActorMessage.IndexDocument |> Actor.post self
        }

    let processMessage (inbox: MailboxProcessor<ActorMessage>) =
        task {
            match! inbox.Receive() with
            | ActorMessage.IndexDocument doc -> do! indexDocument doc
            | _ -> ignore 0
        }

    let rec loop inbox =
        async {
            do! processMessage inbox |> Async.AwaitTask

            return! loop inbox
        }

    let actor = MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox)

    interface IStatsSource with
        member this.GetStatsAsync() =
            task {
                let! queueCount = broker.GetQueueCountAsync Queues.QueueNames.documentIndexing

                let stats = actor |> Actor.getStats (self.GetType().Name)

                return
                    { stats with
                        itemCount = stats.itemCount + (queueCount |> Option.map _.count |> Option.defaultValue 0) }
            }

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg
        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
        member this.Start () = ignore 0
        member this.Stop() = ignore 0
