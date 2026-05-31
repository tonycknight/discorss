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

module Map =
    let getValue (key: 'a) (map: Map<'a, 'b>) =
        match map.TryGetValue key with
        | (true, value) -> Some value
        | _ -> None

    let add (y: Map<'a, int>) (x: Map<'a, int>) = 
        
        y |>
            Map.fold 
                (fun m k i ->   let j = m |> getValue k |> Option.defaultValue 0
                                m |> Map.add k (i + j))                
                x