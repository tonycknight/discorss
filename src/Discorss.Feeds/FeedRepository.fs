namespace Discorss.Feeds

open System
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks

type IFeedRepository =
    abstract member GetFeedInfoAsync: string -> Task<FeedInfo option>
    abstract member GetFeedInfosAsync: unit -> Task<FeedInfo[]>
    abstract member SetFeedInfoAsync: FeedInfo -> Task
    abstract member SetFeedLastUpdateAsync: FeedInfo -> Task

[<ExcludeFromCodeCoverage>]
type StubFeedRepository(feedUris) =

    let feeds =
        feedUris
        |> List.map (fun u ->
            { FeedInfo.uri = u
              title = ""
              updated = DateTimeOffset.MinValue
              lastFetched = DateTimeOffset.MinValue })

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
        member this.GetFeedInfoAsync (uri) =
            task {
                let (ok, feed) = feedCache.TryGetValue(uri)
                return if ok then Some feed else None
            }

        member this.GetFeedInfosAsync() = task { return feedCache.Values |> Seq.toArray }

        member this.SetFeedInfoAsync(feed: FeedInfo) =
            task {
                feedCache.[feed.uri] <-
                    { feed with
                        updated = DateTimeOffset.UtcNow }
            }
        member this.SetFeedLastUpdateAsync(feed: FeedInfo) =
            task {
                let (ok, feed2) = feedCache.TryGetValue(feed.uri)
                let feed = if ok then feed2 else feed
                
                let feed = { feed with updated = DateTimeOffset.UtcNow }

                feedCache.[feed.uri] <- feed
            }