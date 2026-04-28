namespace Discorss.Test.Benchmarks.Feeds

open System.Xml.Linq
open BenchmarkDotNet.Attributes
open Discorss.Feeds

[<MemoryDiagnoserAttribute>]
[<AllStatisticsColumnAttribute>]
[<RankColumn>]
[<JsonExporterAttribute.Full>]
[<GcServerAttribute(true)>]
type Rss092ParserBenchmarks() =


    [<IterationSetup>]
    member this.IterationSetup() =
        this.Text <- "<xml></xml>" // TODO: Discorss.Feeds.Test.Unit.TestHelpers.sampleFeedAsString this.SampleFileName
        this.Doc <- this.Text |> XDocument.Parse

    [<Params("Rss092Feed.xml")>]
    member val SampleFileName = "" with get, set

    member val Text = "" with get, set
    member val Doc = new XDocument() with get, set

    [<Benchmark>]
    member this.Parse() =
        this.Doc |> Rss20Parser.parse "http://url" |> ignore

    [<Benchmark>]
    member this.ParseWithLoad() =
        this.Text |> XDocument.Parse |> Rss20Parser.parse "http://url" |> ignore
