namespace Discorss.Test.Benchmarks.Indexing

open System
open BenchmarkDotNet.Attributes
open Discorss
open Discorss.Documents

[<MemoryDiagnoserAttribute>]
[<AllStatisticsColumnAttribute>]
[<RankColumn>]
[<JsonExporterAttribute.Full>]
[<GcServerAttribute(true)>]
type TokenisationBenchmarks() =

    [<Params(0, 1, 2, 4, 8, 16, 32, 64, 128, 256)>]
    member val Size = 0 with get, set

    member val Text = "" with get, set

    [<GlobalSetup>]
    member this.GlobalSetup() =
        let rng = new Random()
        let alphabet = "abcdefghijklmnopqrstuvwxyz" |> Array.ofSeq
        let pick () = alphabet.[rng.Next(0, alphabet.Length)]

        let createWord size =
            let cs = [| 0..size |] |> Array.map (fun i -> pick ())
            new String(cs)

        let createWords size =
            [ 0..size ] |> Seq.map createWord |> Strings.join " "

        this.Text <- createWords this.Size

    [<Benchmark>]
    member this.``wordSplit``() =
        this.Text |> Tokenisation.wordSplit |> Array.ofSeq
