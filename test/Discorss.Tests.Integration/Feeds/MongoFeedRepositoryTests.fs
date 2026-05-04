namespace Discorss.Tests.Integration.Feeds

open System
open Discorss.Feeds
open Discorss.Tests.Integration
open FsUnit.Xunit

module MongoFeedRepositoryTests =
    

    [<Xunit.Fact>]
    let ``SetFeedInfoAsync writes one``() =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let repo =
                new MongoFeedRepository(opts, TestHelpers.logFactory ()) :> IFeedRepository
        
            let feed = 
                { FeedInfo.uri = $"http://localhost/{Guid.NewGuid()}"
                  title = "test doc title"
                  updated = DateTimeOffset.UtcNow
                  lastFetched = DateTimeOffset.UtcNow }

            let! result = repo.SetFeedInfoAsync feed

            result.uri |> should equal feed.uri
            result.title |> should equal feed.title

        }

    [<Xunit.Fact>]
    let ``SetFeedInfoAsync updates one``() =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let repo =
                new MongoFeedRepository(opts, TestHelpers.logFactory ()) :> IFeedRepository
        
            let feed = 
                { FeedInfo.uri = $"http://localhost/{Guid.NewGuid()}"
                  title = "test doc title"
                  updated = DateTimeOffset.UtcNow.Date |> DateTimeOffset
                  lastFetched = DateTimeOffset.UtcNow.Date |> DateTimeOffset }

            let! result = repo.SetFeedInfoAsync feed

            let feed = 
                { feed with 
                    title = Guid.NewGuid().ToString() 
                    updated = DateTimeOffset.UtcNow.Date |> DateTimeOffset
                    lastFetched = DateTimeOffset.UtcNow.Date |> DateTimeOffset }

            let! result = repo.SetFeedInfoAsync feed

            let! persistedFeed = repo.GetFeedInfoAsync feed.uri

            result.uri |> should equal feed.uri
            result.title |> should equal feed.title
            persistedFeed.Value.updated |> should equal feed.updated
            persistedFeed.Value.lastFetched |> should equal feed.lastFetched
        }

    [<Xunit.Fact>]
    let ``GetFeedInfosAsync returns feed``() =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let repo =
                new MongoFeedRepository(opts, TestHelpers.logFactory ()) :> IFeedRepository
        
            let feed = 
                { FeedInfo.uri = $"http://localhost/{Guid.NewGuid()}"
                  title = "test doc title"
                  updated = DateTimeOffset.UtcNow.Date |> DateTimeOffset
                  lastFetched = DateTimeOffset.UtcNow.Date |> DateTimeOffset }

            let! result = repo.SetFeedInfoAsync feed

            let! persistedFeeds = repo.GetFeedInfosAsync ()

            persistedFeeds.Length |> should greaterThanOrEqualTo 1
            persistedFeeds |> Seq.exists (fun f -> f.uri = feed.uri) |> should equal true
        }

    [<Xunit.Fact>]
    let ``GetFeedInfoAsync returns feed``() =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let repo =
                new MongoFeedRepository(opts, TestHelpers.logFactory ()) :> IFeedRepository
        
            let feed = 
                { FeedInfo.uri = $"http://localhost/{Guid.NewGuid()}"
                  title = "test doc title"
                  updated = DateTimeOffset.UtcNow.Date |> DateTimeOffset
                  lastFetched = DateTimeOffset.UtcNow.Date |> DateTimeOffset }

            let! result = repo.SetFeedInfoAsync feed

            let! persistedFeed = repo.GetFeedInfoAsync feed.uri

            result.uri |> should equal feed.uri
            result.title |> should equal feed.title
            persistedFeed.Value.updated |> should equal feed.updated
            persistedFeed.Value.lastFetched |> should equal feed.lastFetched
        }

    [<Xunit.Fact>]
    let ``SetFeedLastFetchedAsync updates``()=
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let repo =
                new MongoFeedRepository(opts, TestHelpers.logFactory ()) :> IFeedRepository
        
            let date = DateTimeOffset.UtcNow.AddDays(-1) 
            let feed = 
                { FeedInfo.uri = $"http://localhost/{Guid.NewGuid()}"
                  title = "test doc title"
                  updated = date.DateTime |> DateTimeOffset
                  lastFetched = date.DateTime |> DateTimeOffset }

            let! result = repo.SetFeedInfoAsync feed
            
            do! repo.SetFeedLastFetchedAsync feed

            let! persistedFeed = repo.GetFeedInfoAsync feed.uri

            persistedFeed.Value.lastFetched |> should be (greaterThan date)
            persistedFeed.Value.uri |> should equal feed.uri
            persistedFeed.Value.title |> should equal feed.title
            persistedFeed.Value.updated |> should equal feed.updated
        }