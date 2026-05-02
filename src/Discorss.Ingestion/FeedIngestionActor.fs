namespace Discorss.Ingestion

open System
open System.Diagnostics.CodeAnalysis
open Discorss
open Discorss.Feeds
open Discorss.Queues
open Microbroker.Client
open Microsoft.Extensions.Logging

[<ExcludeFromCodeCoverage>]
type FeedIngestionActor(logFactory: ILoggerFactory, feedRepo: IFeedRepository, feedProvider: IFeedProvider, broker: IMicrobrokerProxy) as self =
    let log = logFactory.CreateLogger<FeedIngestionActor>()
        
    let getFeedInfo uri = feedRepo.GetFeedInfoAsync uri

    let getFeed feed =
        task {
            let! r = feedProvider.GetFeedAsync feed.uri
            return!
                match r with
                | FeedReadResult.Feed fr -> 
                    task {
                        do! feedRepo.SetFeedLastUpdateAsync feed
                        return Some fr
                    }
                | FeedReadResult.Error msg -> 
                    task {
                        log.LogError msg
                        return None
                    }
                | FeedReadResult.Xml xml ->
                    task {
                        log.LogError $"Failed to parse feed {feed.uri} - XML: {xml}"
                        return None
                    }
        }
    
    let forwardEntries (feedEntries: FeedEntry list) =
        task {            
            let msgs = feedEntries |> List.map (ActorMessage.FeedEntry >> Messages.toQueueMessage)
                
            do! broker.PostManyAsync (QueueNames.feedEntries, msgs)
        }
        
    let ingestFeed uri =
        task {
            try
                match! getFeedInfo uri with
                | Some feedInfo -> 
                    let! feed = getFeed feedInfo

                    let feedEntries = feed |> Option.map _.entries |> Option.defaultValue [] 
                
                    do! forwardEntries feedEntries
                | None -> 
                    log.LogWarning $"Cannot find feed for {uri}"
            with
            | ex ->
                log.LogError (ex, $"Error ingesting feed {uri}")
        }

    let startIngestion () =
        task {
            let! feeds = feedRepo.GetFeedInfosAsync()                
            feeds 
            |> Array.map (fun f -> f.uri |> ActorMessage.IngestFeed |> (Actor.post self))
            |> ignore
        }

    let rec loop (inbox: MailboxProcessor<ActorMessage>) =
        task {

            match! inbox.Receive() with
            | ActorMessage.Start -> ignore 0
            | ActorMessage.IngestFeeds -> do! startIngestion ()
            | ActorMessage.IngestFeed uri -> do! ingestFeed uri
            | ActorMessage.AddFeed uri -> uri |> ActorMessage.IngestFeed |> inbox.Post
            | ActorMessage.RemoveFeed uri -> ignore 0 // TODO: 
            | ActorMessage.GetActorStats rc -> inbox |> Actor.getStats (self.GetType().Name) |> rc.Reply
            | m -> ignore m // TODO: parent.Post m

            return! loop inbox
        }

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox |> Async.AwaitTask) // TODO: ??

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.GetStats() =
            actor.PostAndAsyncReply(fun rc -> ActorMessage.GetActorStats rc)

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)