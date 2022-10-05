namespace Discorss.Indexing

open Discorss

module Lexicons=

    let stopWords = [ "the"; "these"; "this"; "a"; "an"; "and"; "i"; "we"; "is"; "as"; "be"; "to"; "has"; "for" ]
                        |> Set.ofSeq

    let isStopWord word = 
        stopWords |> Set.contains (Strings.lower word)