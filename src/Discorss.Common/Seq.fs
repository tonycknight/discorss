namespace Discorss

open System

module Seq =
    let counts (xs: seq<'a>) =
        let result = new System.Collections.Generic.Dictionary<'a, int>()

        for x in xs do
            result.[x] <-
                match result.TryGetValue x with
                | (true, x) -> x + 1
                | _ -> 1

        result |> Seq.map (fun kvp -> (kvp.Key, kvp.Value))
