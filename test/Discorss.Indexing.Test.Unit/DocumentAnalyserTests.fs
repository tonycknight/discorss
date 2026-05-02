namespace Discorss.Indexing.Tests.Unit

open System
open Discorss.Indexing
open FsCheck
open FsUnit.Xunit

module DocumentAnalyserTests =

    [<Xunit.Fact>]
    let ``Words for empty document returns empty`` () =
        let lexicon = new Lexicon() :> ILexicon
        let analyser = new DocumentAnalyser(lexicon): IDocumentAnalyser

        let doc =
            { Document.uri = ""
              author = ""
              title = ""
              description = ""
              publication = DateTimeOffset.UtcNow
              categories = [||]
              content = "" }

        let words = analyser.Words(doc) |> Array.ofSeq

        words |> should be Empty

    [<Xunit.Fact>]
    let ``Words for document returns words`` () =
        let lexicon = new Lexicon() :> ILexicon
        let analyser = new DocumentAnalyser(lexicon): IDocumentAnalyser

        let expected = [| "headline"; "joe"; "summary"; "stuff" |]

        let doc =
            { Document.uri = ""
              author = expected.[1]
              title = expected.[0]
              description = expected.[2]
              publication = DateTimeOffset.UtcNow
              categories = [||]
              content = expected.[3] }

        let words = analyser.Words(doc) |> Array.ofSeq

        words |> should equal expected


    [<Xunit.Fact>]
    let ``Statistics for empty document returns empty`` () =
        let lexicon = new Lexicon() :> ILexicon
        let analyser = new DocumentAnalyser(lexicon): IDocumentAnalyser

        let doc =
            { Document.uri = ""
              author = ""
              title = ""
              description = ""
              publication = DateTimeOffset.UtcNow
              categories = [||]
              content = "" }

        let stats = analyser.Statistics(doc)

        stats.wordCount |> should equal 0
        stats.wordFrequencies |> should be Empty

    [<Xunit.Fact>]
    let ``Statistics for document returns counts`` () =
        let lexicon = new Lexicon() :> ILexicon
        let analyser = new DocumentAnalyser(lexicon): IDocumentAnalyser

        let words = [| "headline" |]

        let doc =
            { Document.uri = ""
              author = words.[0]
              title = words.[0]
              description = words.[0]
              publication = DateTimeOffset.UtcNow
              categories = [||]
              content = words.[0] }

        let stats = analyser.Statistics(doc)

        stats.wordCount |> should equal 4
        stats.wordFrequencies.Count |> should equal 1
        stats.wordFrequencies.Item(words.[0]) |> should equal 4
