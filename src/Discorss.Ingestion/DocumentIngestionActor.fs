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
        document

    let hasCacheDelta document =
        match cacheKey document |> cache.TryGetValue with
        | true, x -> (x :?> string) <> document.sha512 |> Some
        | false, _ -> None

    let documentEditDistance (x: Documents.Document) (y: Documents.Document) =

        let abs (x: int) = System.Math.Abs(x)

        let title = Strings.editDistance x.title y.title
        let content = Strings.editDistance x.content y.content
        let desc = Strings.editDistance x.description y.description
        let auth = Strings.editDistance x.author y.author

        abs title + abs content + abs desc + abs auth

    let hasRepoDelta (document: Documents.Document) =
        task {
            try
                let! doc = docRepo.GetDocumentAsync document.uri

                return
                    match doc with
                    | Some doc ->
                        if doc.sha512 <> document.sha512 then
                            let distance = documentEditDistance doc document

                            let msg =
                                $"Document {document.uri} has changed since last ingestion. Edits: {distance}."

                            let (result, msg) =
                                if distance >= config.Value.documentEditDistanceThreshold then
                                    (true, $"{msg} Rebuilding...")
                                else
                                    (false, $"{msg}. Skipping...")

                            msg |> log.LogTrace
                            result
                        else
                            false
                    | None -> true

            with ex ->
                log.LogError(ex, $"Error fetching document {document.uri}")
                return true
        }

    let shouldWriteDocument (document: Documents.Document) =
        task {
            match hasCacheDelta document with
            | None -> return! hasRepoDelta document
            | Some x when x -> return! hasRepoDelta document
            | Some x -> return x
        }

    let logDocumnetReceipt (document: Documents.Document) =
        log.LogInformation $"Starting ingestion for document {document.uri}..."
        document

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

            let message =
                document |> ActorMessage.IndexDocument |> Queues.Messages.toQueueMessage

            do! broker.PostAsync(Queues.QueueNames.documentIndexing, message)
        }

    let processMessage (inbox: MailboxProcessor<ActorMessage>) =
        task {
            match! inbox.Receive() with
            | ActorMessage.FeedEntry fe ->
                let d = Models.toDocument fe

                let! shouldWrite = shouldWriteDocument d

                if shouldWrite then
                    match! d |> logDocumnetReceipt |> writeDocument with
                    | Some d -> do! d |> setCachedDocHash |> forwardDocument
                    | _ -> ignore 0
                else
                    setCachedDocHash d |> ignore
                    log.LogTrace $"Skipping document {d.uri} as already ingested"
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
            actor |> Actor.getStats (self.GetType().Name) |> Task.ofResult

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg
