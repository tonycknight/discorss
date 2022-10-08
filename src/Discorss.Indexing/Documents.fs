namespace Discorss.Indexing

open System
open System.Threading.Tasks
open Discorss

type IDocumentStatsWriter=
    abstract member Set: stats:DocumentStatistics -> Task

type StubDocumentStatsWriter()=
    
    interface IDocumentStatsWriter with
        member this.Set(stats) =
            task {
                do! Task.Delay(0)
            }