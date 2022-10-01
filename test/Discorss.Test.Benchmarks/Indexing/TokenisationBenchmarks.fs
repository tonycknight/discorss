namespace Discorss.Test.Benchmarks.Indexing

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes


[<MemoryDiagnoserAttribute>]
[<AllStatisticsColumnAttribute>]
[<RankColumn>]
[<JsonExporterAttribute.Full>]
[<GcServerAttribute(true)>]
type TokenisationBenchmarks()=
    
    [<Params(0, 1, 2, 4, 8, 16, 32, 64, 128)>]
    member val Count = 0 with get, set

    member val Text = "" with get, set

    [<IterationSetup>]
    member this.Setup()=
        let words = [0 .. this.Count]
                        |> Seq.map (fun i -> new String('a', i))
        this.Text <- String.Join(' ', words)        

    [<Benchmark>]
    member this.``wordSplit``() =
        this.Text |> Discorss.Indexing.Tokenisation.wordSplit |> Array.ofSeq
        
