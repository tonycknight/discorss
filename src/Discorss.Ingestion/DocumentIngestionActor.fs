namespace Discorss.Ingestion

open System.Diagnostics.CodeAnalysis
open Discorss
open Microbroker.Client
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

[<ExcludeFromCodeCoverage>]
type DocumentIngestionActor
    (
        logFactory: ILoggerFactory,
        config: IOptions<AppConfiguration>,
        cache: IMemoryCache,
        docRepo: Documents.IDocumentRepository,
        broker: IMicrobrokerProxy
    ) as self =

    let log = logFactory.CreateLogger<DocumentIngestionActor>()

    let cacheKey (document: Documents.Document) =
        $"{document.GetType().Name}:{document.uri}"

    let setCachedDocHash document =
        let key = cacheKey document

        let options =
            Caching.cacheOptions () |> Caching.expiry config.Value.documentIngestionWindow

        cache.Set(key, document.sha512, options) |> ignore

    let hasCacheDelta document =
        match cacheKey document |> cache.TryGetValue with
        | true, x -> (x :?> string) <> document.sha512
        | _ -> true

    let writeDocument (document: Documents.Document) =
        task {
            try
                log.LogTrace $"Storing document {document.uri}..."
                let! document = docRepo.SetDocumentAsync document
                log.LogTrace $"Stored document {document.uri}."
                return Some document

            with ex ->
                log.LogError(ex, $"Error writing document {document.uri}")
                document |> ActorMessage.Document |> Actor.post self
                return None
        }

    let forwardDocument (document: Documents.Document) =
        task {
            let message = ActorMessage.Document document |> Queues.Messages.toQueueMessage

            do! broker.PostAsync(Queues.QueueNames.documents, message)
        }

    let rec loop (inbox: MailboxProcessor<ActorMessage>) =
        task {
            match! inbox.Receive() with
            | ActorMessage.FeedEntry fe ->
                let d = Models.toDocument fe

                if hasCacheDelta d then
                    match! writeDocument d with
                    | Some d ->
                        do! forwardDocument d
                        setCachedDocHash d
                    | _ -> ignore 0
                else
                    log.LogTrace $"Skipping document {d.uri} as already ingested"
            | _ -> ignore 0

            return! loop inbox
        }

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox |> Async.AwaitTask)

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.GetStats() =
            actor |> Actor.getStats (self.GetType().Name) |> Task.ofResult

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
