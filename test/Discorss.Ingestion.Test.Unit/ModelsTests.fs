namespace Discorss.Ingestion

open Discorss
open Discorss.Feeds
open Discorss.Ingestion
open FsCheck.Xunit
open FsUnit.Xunit

module ModelsTests =

    [<Property>]
    let ``sha512 returns non-empty string`` (value: FeedEntry) =
        let result = Models.sha512 value

        result |> Strings.isEmptyWhitespace |> not

    [<Property>]
    let ``sha512 returns stable string`` (value: FeedEntry) =
        Models.sha512 value = Models.sha512 value

    [<Property>]
    let ``toDocument produces mapped value`` (value: FeedEntry) =

        let result = Models.toDocument value

        result.uri |> should equal value.uri
        result.author |> should equal value.author
        result.categories |> should equalSeq value.categories
        result.content |> should equal value.content
        result.description |> should equal value.description
        result.publication |> should equal value.publication
        result.title |> should equal value.title
        Strings.isEmptyWhitespace result.sha512 |> should equal false

        true
