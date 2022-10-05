namespace Discorss.Indexing.Tests.Unit

open System
open Discorss
open Discorss.Indexing
open FsCheck
open FsCheck.Xunit
open FsUnit

module LexiconsTests=

    [<Xunit.Fact>]
    let ``isStopWord mapped to upper are all true on known members``()=
        
        let hits = Lexicons.stopWords
                    |> Seq.map Strings.upper
                    |> Seq.map Lexicons.isStopWord

        hits |> Seq.exists (fun x -> not x ) |> should equal false

    [<Xunit.Fact>]
    let ``isStopWord mapped to lower are all true on known members``()=
        
        let hits = Lexicons.stopWords
                    |> Seq.map Strings.lower
                    |> Seq.map Lexicons.isStopWord

        hits |> Seq.exists (fun x -> not x ) |> should equal false

    [<Xunit.Fact>]
    let ``isStopWord mapped to mixed are all true on known members``()=
        
        let hits = Lexicons.stopWords
                    |> Seq.map Strings.mixed
                    |> Seq.map Lexicons.isStopWord

        hits |> Seq.exists (fun x -> not x ) |> should equal false


    [<Property(Verbose = true)>]
    let ``isStopWord random are all false``(x: NonEmptyString)=
        
        let hit = x.ToString() |> Lexicons.isStopWord

        hit |> should equal false

    [<Property(Verbose = true)>]
    let ``isStopWord random guid are all false``(x: System.Guid)=
        
        let hit = x.ToString() |> Lexicons.isStopWord

        hit |> should equal false

