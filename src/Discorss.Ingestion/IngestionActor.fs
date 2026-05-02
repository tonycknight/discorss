namespace Discorss.Ingestion

open System
open System.Diagnostics.CodeAnalysis
open Discorss
open Discorss.Queues
open Microbroker.Client
open Microsoft.Extensions.Logging

[<ExcludeFromCodeCoverage>]
type IngestionActor(logFactory: ILoggerFactory, broker: IMicrobrokerProxy, feedActor: FeedIngestionActor) as self =
    
    let log = logFactory.CreateLogger<IngestionActor>()
    let cancellation = new System.Threading.CancellationTokenSource()
    
    let postIngest args =
        ActorMessage.IngestFeeds |> Actor.post self        

    let pollMicrobroker queueName  =
        task {
            while (not cancellation.IsCancellationRequested) do
                let! m = broker.GetNextAsync queueName

                match m |> Option.bind Messages.fromQueueMessage<ActorMessage> with
                | None -> ignore m
                | Some m -> m |> Actor.post self            
        }

    let postIngestTimer = postIngest |> Actor.createTimer (TimeSpan.FromMinutes 1.) 
    
    let getStats inbox = Actor.getStats (self.GetType().Name) inbox

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox ->
            let rec loop () =
                async {
                    let! msg = inbox.Receive()

                    match msg with                    
                    | ActorMessage.Start ->
                        do postIngestTimer.Enabled <- true
                    | ActorMessage.Stop ->
                        do postIngestTimer.Enabled <- false
                        do cancellation.Cancel()                        
                    | ActorMessage.IngestFeeds
                    | ActorMessage.AddFeed _
                    | ActorMessage.RemoveFeed _
                    | ActorMessage.IngestFeed _ ->
                        msg |> Actor.post feedActor
                    | ActorMessage.Feeds _ -> ignore 0
                    | ActorMessage.Documents _
                    | ActorMessage.IndexDoc _ -> 
                        ignore 0 // TODO:
                    | ActorMessage.FeedEntry e ->
                        // do! broker.PostAsync (QueueNames.feedEntries, (e |> Messages.toQueueMessage))
                        ignore 0 // TODO: forward to...?

                    | ActorMessage.GetActorStats rc -> 
                        inbox |> getStats |> rc.Reply // TODO: ewww
                    // TODO: need to pull messages from microbroker queues
                    | m -> ignore 0

                    return! loop ()
                }

            loop ())

    interface IActor with
        member this.GetStats() =
            actor.PostAndAsyncReply(fun rc -> ActorMessage.GetActorStats rc)

        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
