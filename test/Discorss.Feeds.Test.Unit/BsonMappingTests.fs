namespace Discorss.Feeds.Test.Unit

open System
open Discorss.Feeds
open FsCheck.Xunit
open FsUnit.Xunit

module BsonMappingTests =

    [<Property>]
    let ``toBson fromBson is symmetric`` (feed: FeedInfo) =

        let result = feed |> BsonMapping.toBson |> BsonMapping.fromBson

        result.uri |> should equal feed.uri
        result.title |> should equal feed.title
        result.description |> should equal feed.description
        result.updated |> should equal feed.updated
        result.lastFetched |> should equal feed.lastFetched

        true
