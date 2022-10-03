namespace Discorss.Test.Benchmarks.Indexing

open System
open BenchmarkDotNet.Attributes
open Discorss
open Discorss.Indexing

[<MemoryDiagnoserAttribute>]
[<AllStatisticsColumnAttribute>]
[<RankColumn>]
[<JsonExporterAttribute.Full>]
[<GcServerAttribute(true)>]
type TokenisationBenchmarks()=
    
    [<Params(0, 1, 2, 4, 8, 16, 32, 64, 128, 256)>]
    member val Count = 0 with get, set

    member val Text = "" with get, set

    [<IterationSetup>]
    member this.Setup()=
        this.Text <- [0 .. this.Count]
                        |> Seq.map (fun i -> new String('a', i))
                        |> Strings.join " "

    [<Benchmark>]
    member this.``wordSplit``() =
        this.Text |> Tokenisation.wordSplit |> Array.ofSeq
        
