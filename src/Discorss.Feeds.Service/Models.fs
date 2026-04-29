namespace Discorss.Feeds.Service

open Discorss.Feeds
open Discorss.ApiModels

module Mapping =
    let toFeedInfoApiModel (value: Discorss.Feeds.FeedInfo) =
        { Discorss.ApiModels.FeedInfo.uri = value.uri;
          title = value.title;
          description = "";
          updated = value.updated;
          lastFetched = value.lastFetched; }

    let toFeedApiModel (value: Discorss.Feeds.Feed ) =
        { Discorss.ApiModels.Feed.feed =
            { uri = value.uri
              title = value.title
              description = value.description
              updated = value.updated
              lastFetched = System.DateTimeOffset.UtcNow }
          entries =
            value.entries
            |> Seq.map (fun e ->
                { id = e.id
                  publication = e.publication
                  uri = e.uri
                  title = e.title
                  description = e.description
                  author = e.author
                  content = e.content
                  categories = e.categories })
            |> Array.ofSeq }