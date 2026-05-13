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
                
                do! statsRepo.SetAsync stats
                log.LogTrace($"Statistics written for document {doc.uri}.")

            with ex ->
                log.LogError(ex, $"Error calculating statistics for document {doc.uri}")
                doc |> ActorMessage.IndexDocument |> Actor.post self
        }

    let processMessage (inbox: MailboxProcessor<ActorMessage>) =
        task {
            match! inbox.Receive() with
            | ActorMessage.IndexDocument doc ->
                do! indexDocument doc                
            | _ -> ignore 0
        }

    let rec loop inbox =
        async {
            do! processMessage inbox |> Async.AwaitTask

            return! loop inbox
        }
            
    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox)

    interface IStatsSource with
        member this.GetStatsAsync() =
            // TODO: query queue?
            actor |> Actor.getStats (self.GetType().Name) |> Task.ofResult

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
