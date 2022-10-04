namespace Discorss.Indexing


type Document = {
    uri:                string
    author:             string
    title:              string
    description:        string
    content:            string
    }


type DocumentStatistics = {
    uri:                string
    wordFrequencies:    (string * int)[]
    }