namespace Discorss.Feeds

open System

type FeedType=
    | Rss20
    | Rss091
    | Rss092
    | Atom
    | Unknown

type FeedEntry = {
    id:             string
    publication:    DateTimeOffset
    uri:            string
    title:          string
    description:    string
    author:         string
    content:        string
    categories:     string list
    }
    
type Feed = {
    feedType:       FeedType
    title:          string
    uri:            string
    description:    string
    updated:        DateTimeOffset
    entries:        FeedEntry list
    }

type FeedInfo = {
    uri:            string
    description:    string
    updated:        DateTimeOffset
    }

type FeedReadResult = 
    | Xml  of doc:System.Xml.Linq.XDocument
    | Feed  of feed:Feed
    | Error of message:string
