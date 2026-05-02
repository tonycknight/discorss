namespace Discorss.Ingestion

open System.Diagnostics.CodeAnalysis
open Discorss
open Microsoft.Extensions.Logging

[<ExcludeFromCodeCoverage>]
type DocumentIngestionActor(logFactory: ILoggerFactory, docRepo: Documents.IDocumentRepository) as self =

    let log = logFactory.CreateLogger<DocumentIngestionActor>()

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
            
    let rec loop (inbox: MailboxProcessor<ActorMessage>) =
        task {
            match! inbox.Receive() with
            | ActorMessage.Document d -> 
                let! d = writeDocument d 
                ignore d
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
