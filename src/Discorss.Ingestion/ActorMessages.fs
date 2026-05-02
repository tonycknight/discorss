namespace Discorss.Ingestion

type ActorStats =
    { name: string
      queueCount: int
      childStats: ActorStats list }

type ActorMessage =
    | Stop
    | Start
    | GetFeeds
    | QueryFeeds of AsyncReplyChannel<string[]>
    | Feeds of urls: Discorss.Feeds.FeedInfo[]
    | AddFeed of url: string
    | RemoveFeed of url: string
    | IngestFeed of url: string
    | IngestFeeds
    | FeedEntry of entry: Discorss.Feeds.FeedEntry
    | Documents of docs: Discorss.Indexing.Document[]
    | IndexDoc of doc: Discorss.Indexing.Document
    | GetActorStats of rc: AsyncReplyChannel<ActorStats>
    | ActorStats of stats: ActorStats
    | PollQueue of queueName: string