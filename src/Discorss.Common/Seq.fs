namespace Discorss

open System

module Seq =
    let counts (xs: seq<'a>) =

        let grps = xs |> Seq.groupBy id

        grps |> Seq.map (fun (k, ys) -> (k, ys |> Seq.length)) |> Map.ofSeq

module Dictionary =
    open System.Collections.Generic

    let ofMap (value: Map<'a, 'b>) = Dictionary<'a, 'b>(value)

    let toMap (value: IDictionary<'a, 'b>) =
        value |> Seq.fold (fun map kvp -> map |> Map.add kvp.Key kvp.Value) Map.empty
