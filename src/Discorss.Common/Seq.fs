namespace Discorss

open System

module Seq =
    let counts (xs: seq<'a>) =

        let grps = xs |> Seq.groupBy id

        grps |> Seq.map (fun (k, ys) -> (k, ys |> Seq.length)) |> Map.ofSeq
