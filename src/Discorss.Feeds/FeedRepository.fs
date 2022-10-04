namespace Discorss.Feeds

open System
open System.Threading.Tasks

type IFeedRepository=
    abstract member GetFeedsAsync: unit -> Task<seq<FeedInfo>>

type StubFeedRepository(feedUris)=
    
    let feeds = feedUris |> List.map (fun u -> { FeedInfo.uri = u; description = ""; updated = DateTimeOffset.MinValue })
    
    let feedCache = feeds   |> Seq.map (fun f -> new System.Collections.Generic.KeyValuePair<string, FeedInfo>(f.uri, f))
                            |> System.Collections.Concurrent.ConcurrentDictionary<string, FeedInfo>
    
    new() = StubFeedRepository([
                                    "https://devblogs.microsoft.com/dotnet/feed/";
                                    "https://azure.microsoft.com/en-gb/blog/feed/";
                                    "https://devblogs.microsoft.com/cosmosdb/feed/";
                                    "https://devblogs.microsoft.com/dotnet/tag/f/feed/"
                                ])

    interface IFeedRepository with
        member this.GetFeedsAsync()=
            task {                
                return feedCache.Values 
            }
