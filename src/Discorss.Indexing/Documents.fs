namespace Discorss.Indexing

open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks
open Discorss.Documents

type IDocumentStatsWriter =
    abstract member Set: stats: DocumentStatistics -> Task

[<ExcludeFromCodeCoverage>]
type StubDocumentStatsWriter() =

    interface IDocumentStatsWriter with
        member this.Set(stats) = task { do! Task.Delay(0) }

[<ExcludeFromCodeCoverage>]
type MmemoryDocumentStatsWriter() =

    // specific doc counts actor
    // word counts flipped actor

    interface IDocumentStatsWriter with
        member this.Set(stats) = task { do! Task.Delay(0) }
