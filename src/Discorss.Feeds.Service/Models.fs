namespace Discorss.Feeds.Service


[<CLIMutable>]
type PreviewFeedRequest = {
    uri:    string
    }

[<CLIMutable>]
type PreviewFeedResponse = {
    uri:        string
    feed:       Discorss.Feeds.Feed option
    messages:   string list
    }

