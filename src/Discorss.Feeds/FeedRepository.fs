namespace Discorss.Feeds

open System
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks

type IFeedRepository =
    abstract member GetFeedInfosAsync: unit -> Task<seq<FeedInfo>>
    abstract member SetFeedInfoAsync: FeedInfo -> Task

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
              "https://devblogs.microsoft.com/dotnet/tag/f/feed/" ]
        )

    interface IFeedRepository with
        member this.GetFeedInfosAsync() = task { return feedCache.Values }

        member this.SetFeedInfoAsync(feed: FeedInfo) =
            task {
                feedCache.[feed.uri] <-
                    { feed with
                        updated = DateTimeOffset.UtcNow }
            }
