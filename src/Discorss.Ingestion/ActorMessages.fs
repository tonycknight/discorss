namespace Discorss.Ingestion

type ActorStats =
    { name: string
      queueCount: int64
      childStats: ActorStats list }

type ActorMessage =
    | Stop
    | Start
    | GetFeeds
    | IngestFeed of url: string
    | IngestFeeds
    | FeedEntry of entry: Discorss.Feeds.FeedEntry
    | Document of docs: Discorss.Documents.Document
    | DocumentNotification of uri: string
    | IndexDocument of doc: Discorss.Documents.Document
    | ActorStats of stats: ActorStats
    | PollQueue of queueName: string
