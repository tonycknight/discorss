namespace Discorss.Test.Benchmarks.Indexing

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes


[<MemoryDiagnoserAttribute>]
[<AllStatisticsColumnAttribute>]
[<RankColumn>]
[<JsonExporterAttribute.Full>]
[<GcServerAttribute(true)>]
type IndexingBenchmarks()=
    

    [<Benchmark>]
    member _.Do() =
        0 |> ignore
