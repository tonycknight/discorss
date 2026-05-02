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
        
    let postIngestTimer = (fun args -> ActorMessage.IngestFeeds |> Actor.post self) 
                            |> Actor.createTimer (TimeSpan.FromSeconds 15.) 
    let postPollTimer = (fun args -> ActorMessage.PollQueue QueueNames.feedEntries |> Actor.post self)
                            |> Actor.createTimer (TimeSpan.FromSeconds 5.)

    let getStats inbox = Actor.getStats (self.GetType().Name) inbox

    let rec loop (inbox: MailboxProcessor<ActorMessage>) =
        task {
            let! msg = inbox.Receive()

            match msg with
            | ActorMessage.PollQueue queueName -> // TODO: should be on a different thread/actor as polling blocks this
                log.LogTrace $"Polling queue {queueName}..."
                do! Actor.pollEntryQueue broker queueName (Actor.post self)
            | ActorMessage.Start ->
                do postIngestTimer.Enabled <- true
                do postPollTimer.Enabled <-  true
            | ActorMessage.Stop ->
                do postIngestTimer.Enabled <- false
                do postPollTimer.Enabled <- false
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
                log.LogTrace $"Received feedentry {e.title}..."
                ignore 0 // TODO: forward to...?

            | ActorMessage.GetActorStats rc -> 
                inbox |> getStats |> rc.Reply // TODO: ewww
            // TODO: need to pull messages from microbroker queues
            | m -> ignore 0

            return! loop inbox
        }

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox |> Async.AwaitTask)

    interface IActor with
        member this.GetStats() =
            actor.PostAndAsyncReply(fun rc -> ActorMessage.GetActorStats rc)

        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
