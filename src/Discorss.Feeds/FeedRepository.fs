namespace Discorss.Feeds

open System
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open MongoDB.Bson

type IFeedRepository =
    abstract member GetFeedInfoAsync: string -> Task<FeedInfo option>
    abstract member GetFeedInfosAsync: unit -> Task<FeedInfo[]>
    abstract member SetFeedInfoAsync: FeedInfo -> Task<FeedInfo>
    abstract member SetFeedLastFetchedAsync: FeedInfo -> Task

[<ExcludeFromCodeCoverage>]
type StubFeedRepository(feedUris) =

    let feeds =
        feedUris
        |> List.map (fun u ->
            { FeedInfo.uri = u
              title = ""
              description = ""
              updated = DateTime.MinValue
              lastFetched = DateTime.MinValue })

    let feedCache =
        feeds
        |> Seq.map (fun f -> new System.Collections.Generic.KeyValuePair<string, FeedInfo>(f.uri, f))
        |> System.Collections.Concurrent.ConcurrentDictionary<string, FeedInfo>

    new() =
        StubFeedRepository(
            [ "https://devblogs.microsoft.com/dotnet/feed/"
              "https://azure.microsoft.com/en-gb/blog/feed/"
              "https://devblogs.microsoft.com/cosmosdb/feed/"
              "https://devblogs.microsoft.com/dotnet/tag/f/feed/"
              "https://github.blog/changelog/feed/"
              "https://github.blog/latest/feed/"
              "https://rss.slashdot.org/Slashdot/slashdotMain" ]
        )

    interface IFeedRepository with
        member this.GetFeedInfoAsync(uri) =
            task {
                let (ok, feed) = feedCache.TryGetValue(uri)
                return if ok then Some feed else None
            }

        member this.GetFeedInfosAsync() =
            task { return feedCache.Values |> Seq.toArray }

        member this.SetFeedInfoAsync(feed: FeedInfo) =
            task {
                feedCache.[feed.uri] <- { feed with updated = DateTime.UtcNow }

                return feed
            }

        member this.SetFeedLastFetchedAsync(feed: FeedInfo) =
            task {
                let (ok, feed2) = feedCache.TryGetValue(feed.uri)
                let feed = if ok then feed2 else feed

                let feed = { feed with updated = DateTime.UtcNow }

                feedCache.[feed.uri] <- feed
            }

type MongoFeedRepository(config: IOptions<AppConfiguration>, logFactory: ILoggerFactory) =

    [<Literal>]
    let colName = "Feeds"

    let log = logFactory.CreateLogger<MongoFeedRepository>()

    let collection =
        Mongo.initCollection "uri" config.Value.mongoDbName colName config.Value.mongoConnection

    interface IFeedRepository with
        member this.GetFeedInfosAsync() : Task<FeedInfo array> =
            task {

                let! xs = "{}" |> Mongo.getMany<BsonDocument> collection

                return xs |> Seq.map BsonMapping.fromBson |> Array.ofSeq
            }

        member this.GetFeedInfoAsync(key: string) : Task<FeedInfo option> =
            task {
                let! xs = $"{{ _id: '{key}' }}" |> Mongo.getMany<BsonDocument> collection

                return xs |> Seq.map BsonMapping.fromBson |> Seq.tryHead
            }

        member this.SetFeedInfoAsync(feed: FeedInfo) =
            task {
                let! result = feed |> BsonMapping.toBson |> Mongo.upsert collection

                if not result.IsAcknowledged then
                    new Exception("Set not acknowledged") |> raise

                return feed
            }

        member this.SetFeedLastFetchedAsync(feed: FeedInfo) : Task =
            task {
                let this = (this :> IFeedRepository)
                let! persistedFeed = this.GetFeedInfoAsync feed.uri

                match persistedFeed with
                | None -> ignore 0
                | Some feed ->
                    let feed =
                        { feed with
                            lastFetched = DateTime.UtcNow }

                    let! x = this.SetFeedInfoAsync feed
                    ignore 0
            }
