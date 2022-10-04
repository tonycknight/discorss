namespace Discorss.Indexing

open Discorss

module Lexicons=

    let stopWords = [ "the"; "a"; "an"; "and"; "i"; "we"; "is"; "as"; "be"; "to"; "has" ]
                        |> Set.ofSeq

    let isStopWord word = 
        stopWords |> Set.contains (Strings.lower word)