namespace Discorss.Documents

open System
open System.Threading.Tasks

type IWordStatisticsRepository =
    abstract member AddAsync: DocumentStatistics -> Task

type StubWordStatisticsRepository() =

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

type StubDocumentStatisticsRepository() =

    interface IDocumentStatisticsRepository with
        member this.AddAsync(stats: DocumentStatistics) =
            task {
                // TODO:
                return 0
            }
