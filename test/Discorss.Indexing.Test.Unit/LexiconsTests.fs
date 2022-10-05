namespace Discorss.Indexing.Tests.Unit

open System
open Discorss
open Discorss.Indexing
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit

module LexiconsTests=

    [<Xunit.Fact>]
    let ``IsStopWord mapped to upper are all true on known members``()=
        
        let lexicon = (new Lexicon() :> ILexicon)
        let hits = lexicon.StopWords()
                    |> Seq.map Strings.upper
                    |> Seq.map lexicon.IsStopWord

        hits |> Seq.exists (fun x -> not x ) |> should equal false

    [<Xunit.Fact>]
    let ``IsStopWord mapped to lower are all true on known members``()=
        let lexicon = (new Lexicon() :> ILexicon)
        let hits = lexicon.StopWords()
                    |> Seq.map Strings.lower
                    |> Seq.map lexicon.IsStopWord

        hits |> Seq.exists (fun x -> not x ) |> should equal false

    [<Xunit.Fact>]
    let ``IsStopWord mapped to mixed are all true on known members``()=
        let lexicon = (new Lexicon() :> ILexicon)
        let hits = lexicon.StopWords()
                    |> Seq.map Strings.mixed
                    |> Seq.map lexicon.IsStopWord

        hits |> Seq.exists (fun x -> not x ) |> should equal false


    [<Property(Verbose = true)>]
    let ``IsStopWord random guid are all false``(x: System.Guid)=
        let lexicon = (new Lexicon() :> ILexicon)

        let hit = x.ToString() |> lexicon.IsStopWord

        hit |> should equal false


    [<Xunit.Fact>]
    let ``IsKnownWord mapped to upper are all true on known members``()=
        
        let lexicon = (new Lexicon() :> ILexicon)
        let hits = lexicon.KnownWords()
                    |> Seq.map Strings.upper
                    |> Seq.map lexicon.IsKnownWord

        hits |> Seq.exists (fun x -> not x ) |> should equal false

    [<Xunit.Fact>]
    let ``IsKnownWord mapped to lower are all true on known members``()=
        let lexicon = (new Lexicon() :> ILexicon)
        let hits = lexicon.KnownWords()
                    |> Seq.map Strings.lower
                    |> Seq.map lexicon.IsKnownWord

        hits |> Seq.exists (fun x -> not x ) |> should equal false

    [<Xunit.Fact>]
    let ``IsKnownWord mapped to mixed are all true on known members``()=
        let lexicon = (new Lexicon() :> ILexicon)
        let hits = lexicon.KnownWords()
                    |> Seq.map Strings.mixed
                    |> Seq.map lexicon.IsKnownWord

        hits |> Seq.exists (fun x -> not x ) |> should equal false


    [<Property(Verbose = true)>]
    let ``IsKnownWord random are all false``(x: NonEmptyString)=
        let lexicon = (new Lexicon() :> ILexicon)
        
        let hit = x.ToString() |> lexicon.IsKnownWord

        hit |> should equal false

    [<Property(Verbose = true)>]
    let ``IsKnownWord random guid are all false``(x: System.Guid)=
        let lexicon = (new Lexicon() :> ILexicon)

        let hit = x.ToString() |> lexicon.IsKnownWord

        hit |> should equal false

