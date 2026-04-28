namespace Discorss.Feeds.Test.Unit

open System
open Discorss
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit

module SeqTests =

    [<Property(Verbose = true)>]
    let ``counts same value`` (PositiveInt count) =

        let xs = [ 1..count ] |> Seq.map (fun _ -> 0)

        let result = xs |> Seq.counts

        result |> Map.count |> should equal 1
        result |> Seq.head |> _.Value |> should equal count

        true

    [<Property(Verbose = true)>]
    let ``counts different values`` (PositiveInt count) =

        let xs = [ 1..count ] |> Seq.collect (fun x -> [ x; x ])

        let result = xs |> Seq.counts

        result |> Map.count |> should equal count
        result |> Seq.map _.Value |> Seq.min |> should equal 2
        result |> Seq.map _.Value |> Seq.max |> should equal 2

        true
