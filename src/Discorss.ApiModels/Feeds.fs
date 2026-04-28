namespace Discorss.ApiModels

open System

type FeedType =
    | Unknown = 0
    | Rss20 = 1
    | Rss091 = 2
    | Rss092 = 3
    | Atom = 4

[<CLIMutable>]
type FeedInfo =
    { uri: string
      feedType: FeedType
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
      categories: string list }
