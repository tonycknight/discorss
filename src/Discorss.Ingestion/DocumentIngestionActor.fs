namespace Discorss.Ingestion

open System.Diagnostics.CodeAnalysis
open Discorss
open Microbroker.Client
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.Logging

[<ExcludeFromCodeCoverage>]
type DocumentIngestionActor(logFactory: ILoggerFactory, config: AppConfiguration, cache: IMemoryCache, docRepo: Documents.IDocumentRepository, broker: IMicrobrokerProxy) as self =

    let log = logFactory.CreateLogger<DocumentIngestionActor>()
        
    let cacheKey (document: Documents.Document) =
        $"{document.GetType().Name}:{document.uri}"

    let setCachedDocHash document =
        let key = cacheKey document
        let options = Caching.cacheOptions () |> Caching.expiry config.documentIngestionWindow
        cache.Set(key, document.sha512, options) |> ignore

    let hasCacheDelta document = 
        match cacheKey document |> cache.TryGetValue with
        | true, x -> (x :?> string) <> document.sha512
        | _ -> true
        
    let writeDocument (document: Documents.Document) =
        task {
            try
                log.LogTrace $"Storing document {document.uri}..."
                do! docRepo.SetDocumentAsync document
                log.LogTrace $"Stored document {document.uri}."
                return Some document

            with
            | ex ->
                log.LogError (ex, $"Error writing document {document.uri}")
                document |> ActorMessage.Document |> Actor.post self
                return None
        }
            
    let forwardDocument (document: Documents.Document) = 
        task {
            let message = ActorMessage.Document document |> Queues.Messages.toQueueMessage
                
            do! broker.PostAsync (Queues.QueueNames.documents, message)
        }

    let rec loop (inbox: MailboxProcessor<ActorMessage>) =
        task {
            match! inbox.Receive() with
            | ActorMessage.Document d -> 
                if hasCacheDelta d then
                    match! writeDocument d with
                    | Some d -> 
                        do! forwardDocument d
                        setCachedDocHash d
                    | _ -> ignore 0
                
            | ActorMessage.GetActorStats rc -> inbox |> Actor.getStats (self.GetType().Name) |> rc.Reply
            | _ -> ignore 0

            return! loop inbox
        }

    let actor = MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox |> Async.AwaitTask)

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.GetStats() =
            actor.PostAndAsyncReply(fun rc -> ActorMessage.GetActorStats rc)

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
