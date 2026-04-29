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