namespace Discorss.Indexing

open System

type DocumentStatistics =
    { uri: string
      wordCount: int
      wordFrequencies: Map<string, int> }

type WordStatistics = { word: string; wordCounts: int }
