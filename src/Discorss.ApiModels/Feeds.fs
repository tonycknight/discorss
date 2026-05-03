namespace Discorss.ApiModels

open System

[<CLIMutable>]
type FeedInfo =
    { uri: string
      title: string
      description: string
      updated: DateTimeOffset
      lastFetched: DateTimeOffset }

[<CLIMutable>]
type FeedEntry =
    { id: string
      publication: DateTimeOffset
      uri: string
      title: string
      description: string
      author: string
      content: string
      categories: string[] }

[<CLIMutable>]
type Feed =
    { feed: FeedInfo; entries: FeedEntry[] }
