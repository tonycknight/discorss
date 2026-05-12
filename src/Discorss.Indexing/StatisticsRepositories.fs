namespace Discorss.Indexing

open System
open System.Threading.Tasks
open Discorss.Documents

type IWordStatisticsRepository =
    abstract member AddAsync: DocumentStatistics -> Task

type MemoryWordStatisticsRepository() =

    let cache =
        new System.Collections.Concurrent.ConcurrentDictionary<string, WordStatistics>(
            StringComparer.InvariantCultureIgnoreCase
        )

    interface IWordStatisticsRepository with
        member this.AddAsync(stats: DocumentStatistics) =
            task {
                // TODO:
                return 0
            }

type IDocumentStatisticsRepository =
    abstract member AddAsync: DocumentStatistics -> Task

type MemoryDocumentStatisticsRepository() =

    interface IWordStatisticsRepository with
        member this.AddAsync(stats: DocumentStatistics) =
            task {
                // TODO:
                return 0
            }
