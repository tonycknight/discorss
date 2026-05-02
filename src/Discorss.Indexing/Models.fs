namespace Discorss.Indexing

open System

type Document =
    { uri: string
      publication: DateTimeOffset
      author: string
      title: string
      description: string
      content: string
      categories: string[] }

type DocumentStatistics =
    { uri: string
      wordCount: int
      wordFrequencies: Map<string, int> }

type WordStatistics = { word: string; wordCounts: int }
