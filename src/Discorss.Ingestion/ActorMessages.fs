namespace Discorss.Ingestion

type ActorMessage =
| Stop
| Start
| GetFeeds
| Feeds of urls:Discorss.Feeds.FeedInfo[]
| AddFeed of url:string
| RemoveFeed of url:string
| QueryFeed of url:string
| Documents of docs:Discorss.Indexing.Document[]
| IndexDoc of doc:Discorss.Indexing.Document
