namespace Discorss.Ingestion

open Discorss

type ActorMessage =
    | Stop
    | Start
    | GetFeeds
    | IngestFeed of url: string
    | IngestFeeds
    | FeedEntry of entry: Discorss.Feeds.FeedEntry
    | Document of docs: Discorss.Documents.Document
    | DocumentNotification of uri: string
    | DocumentStatistics of stats: Discorss.Documents.DocumentStatistics
    | IndexDocument of doc: Discorss.Documents.Document
    | ActorStats of stats: Stats
    | PollQueue of queueName: string
    