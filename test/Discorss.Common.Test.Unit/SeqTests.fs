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

        let result = xs |> Seq.counts |> Array.ofSeq

        result |> should haveLength 1
        result |> Seq.head |> snd |> should equal count

        true

    [<Property(Verbose = true)>]
    let ``counts different values`` (PositiveInt count) =

        let xs = [ 1..count ] |> Seq.collect (fun x -> [ x; x ])

        let result = xs |> Seq.counts |> Array.ofSeq

        result |> should haveLength count
        result |> Seq.map snd |> Seq.min |> should equal 2
        result |> Seq.map snd |> Seq.max |> should equal 2

        true
