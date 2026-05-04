namespace Discorss.Feeds

open System

type FeedType =
    | Rss20
    | Rss091
    | Rss092
    | Atom
    | Unknown

type FeedEntry =
    { id: string
      publication: DateTime
      uri: string
      title: string
      description: string
      author: string
      content: string
      categories: string[] }

type Feed =
    { feedType: FeedType
      title: string
      uri: string
      description: string
      updated: DateTime
      entries: FeedEntry list }

type FeedInfo =
    { uri: string
      title: string
      description: string
      updated: DateTime
      lastFetched: DateTime }

type FeedReadResult =
    | Xml of doc: System.Xml.Linq.XDocument
    | Feed of feed: Feed
    | Error of message: string

module BsonMapping =
    open Discorss.MongoBson
    open MongoDB.Bson

    let toBson (feed: FeedInfo) =
        newObject ()
        |> setDocId (value feed.uri)
        |> setProperty "uri" (value feed.uri)
        |> setProperty "title" (value feed.title)
        |> setProperty "description" (value feed.description)
        |> setProperty "updated" (value feed.updated)
        |> setProperty "lastFetched" (value feed.lastFetched)

    let fromBson (document: BsonDocument) =
        let asString key = getProperty key >> asString

        { FeedInfo.uri = document |> asString "uri"
          title = document |> asString "title"
          description = document |> asString "description"
          updated = document |> getProperty "updated" |> asDateTime
          lastFetched = document |> getProperty "lastFetched" |> asDateTime }
