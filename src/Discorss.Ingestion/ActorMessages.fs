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
    | FetchFeed of url: string
    | IngestFeeds
    // TODO: | IngestFeed
    | Documents of docs: Discorss.Indexing.Document[]
    | IndexDoc of doc: Discorss.Indexing.Document
    | GetActorStats of rc: AsyncReplyChannel<ActorStats>
    | ActorStats of stats: ActorStats
