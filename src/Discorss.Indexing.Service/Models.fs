namespace Discorss.Indexing.Service


[<CLIMutable>]
type IndexFeedRequest = {
    uri:    string
    }

[<CLIMutable>]
type IndexFeedResponse = {
    uri:        string
    feed:       Discorss.Feeds.Feed option
    message:    string option
    }