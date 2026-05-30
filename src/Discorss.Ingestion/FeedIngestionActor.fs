namespace Discorss.Ingestion

open System
open System.Diagnostics.CodeAnalysis
open Discorss
open Discorss.Feeds
open Discorss.Queues
open Microbroker.Client
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

[<ExcludeFromCodeCoverage>]
type FeedIngestionActor
    (
        logFactory: ILoggerFactory,
        config: IOptions<AppConfiguration>,
        feedRepo: IFeedRepository,
        feedProvider: IFeedProvider,
        broker: IMicrobrokerProxy
    ) as self =
    let log = logFactory.CreateLogger<FeedIngestionActor>()
    let postTimerFrequency = TimeSpan.FromSeconds 15.

    let postIngestTimer =
        (fun args -> ActorMessage.IngestFeeds |> Actor.post self)
        |> Actor.createTimer postTimerFrequency

    let getFeedInfo uri = feedRepo.GetFeedInfoAsync uri

    let getFeed feed =
        task {
            let! r = feedProvider.GetFeedAsync feed.uri

            return!
                match r with
                | FeedReadResult.Feed fr ->
                    task {
                        do! feedRepo.SetFeedLastFetchedAsync feed
                        return Some fr
                    }
                | FeedReadResult.Error msg ->
                    task {
                        log.LogError $"Failed to parse feed {feed.uri}: {msg}"
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
            let msgs =
                feedEntries |> Seq.map (ActorMessage.FeedEntry >> Messages.toQueueMessage)

            do! broker.PostManyAsync(QueueNames.feedEntries, msgs)
        }

    let needsRefresh (feed: FeedInfo) =
        let exp = feed.lastFetched + config.Value.feedIngestionFrequency
        exp < DateTime.UtcNow

    let ingestFeed uri =
        task {
            try
                log.LogTrace $"Starting feed ingestion for {uri}..."

                match! getFeedInfo uri with
                | Some feedInfo when needsRefresh feedInfo ->
                    let! feed = getFeed feedInfo

                    do! feed |> Option.map _.entries |> Option.defaultValue [] |> forwardEntries

                    log.LogTrace $"Completed feed ingestion for {uri}."
                | Some feedInfo -> log.LogTrace $"Feed {feedInfo.uri} not yet aged."
                | None -> log.LogWarning $"Cannot find feed for {uri}"
            with ex ->
                log.LogError(ex, $"Error ingesting feed {uri}")
        }

    let startIngestion () =
        task {
            try
                postIngestTimer.Enabled <- false
                log.LogTrace "Starting feed ingestion..."

                let! feeds = feedRepo.GetFeedInfosAsync()

                feeds
                |> Array.iter (fun f -> f.uri |> ActorMessage.IngestFeed |> (Actor.post self))

                log.LogTrace $"Initiated {feeds.Length} feed ingestions."
            with ex ->
                log.LogError(ex, "Error starting feed ingestion.")

            postIngestTimer.Enabled <- true
        }

    let processMessage (inbox: MailboxProcessor<ActorMessage>) =
        task {
            let! msg = inbox.Receive()

            match msg with
            | ActorMessage.Start -> do postIngestTimer.Enabled <- true
            | ActorMessage.Stop rc ->
                do postIngestTimer.Enabled <- false
                // TODO: prevent further actions
                rc.Reply()
            | ActorMessage.IngestFeeds -> do! startIngestion ()
            | ActorMessage.IngestFeed uri -> do! ingestFeed uri
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
        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)

        member this.Stop() =
            actor.PostAndReply(fun rc -> ActorMessage.Stop rc)
