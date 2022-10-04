namespace Discorss.Indexing.Tests.Unit

open System
open Discorss
open Discorss.Indexing
open FsUnit
open Xunit

module LexiconsTests=

    [<Fact>]
    let ``isStopWord mapped to upper are all true on known members``()=
        
        let hits = Lexicons.stopWords
                    |> Seq.map Strings.upper
                    |> Seq.map Lexicons.isStopWord

        hits |> Seq.exists (fun x -> not x ) |> should equal false

    [<Fact>]
    let ``isStopWord mapped to random are all false``()=
        
        let hits = Lexicons.stopWords
                    |> Seq.map (fun _ -> System.Guid.NewGuid().ToString())
                    |> Seq.map Lexicons.isStopWord

        hits |> Seq.exists (fun x -> x ) |> should equal false

