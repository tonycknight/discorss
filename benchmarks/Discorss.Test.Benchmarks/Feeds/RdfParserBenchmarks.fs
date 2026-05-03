namespace Discorss.Test.Benchmarks.Feeds

open System.Xml.Linq
open BenchmarkDotNet.Attributes
open Discorss.Feeds
open Discorss.Test.Benchmarks

[<MemoryDiagnoserAttribute>]
[<AllStatisticsColumnAttribute>]
[<RankColumn>]
[<JsonExporterAttribute.Full>]
[<GcServerAttribute(true)>]
type RdfParserBenchmarks() =

    [<GlobalSetup>]
    member this.GlobalSetup() =
        this.Text <- TestHelpers.sampleFeedAsString this.SampleFileName
        this.Doc <- this.Text |> XDocument.Parse

    [<Params("RdfFeed.xml")>]
    member val SampleFileName = "" with get, set

    member val Text = "" with get, set
    member val Doc = new XDocument() with get, set

    [<Benchmark>]
    member this.Parse() =
        this.Doc |> RdfParser.parse "http://url" |> ignore

    [<Benchmark>]
    member this.ParseWithLoad() =
        this.Text |> XDocument.Parse |> RdfParser.parse "http://url" |> ignore
