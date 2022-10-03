namespace Discorss.Feeds

open System

type FeedType=
    | Atom
    | Rss091
    | Unknown

type FeedEntry = {
    id:             string
    publication:    DateTimeOffset
    url:            string
    title:          string
    description:    string
    author:         string
    content:        string
    categories:     string list
    }
    
type Feed = {
    feedType:       FeedType
    title:          string
    url:            string
    description:    string
    updated:        DateTimeOffset
    entries:        FeedEntry list
    }

type FeedInfo = {
    url:            string
    description:    string
    updated:        DateTimeOffset
    }