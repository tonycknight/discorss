namespace Discorss.ApiModels

open System

[<CLIMutable>]
type FeedInfo =
    { uri: string
      title: string
      description: string
      updated: DateTime
      lastFetched: DateTime }

[<CLIMutable>]
type FeedEntry =
    { id: string
      publication: DateTime
      uri: string
      title: string
      description: string
      author: string
      content: string
      categories: string[] }

[<CLIMutable>]
type Feed =
    { feed: FeedInfo; entries: FeedEntry[] }
