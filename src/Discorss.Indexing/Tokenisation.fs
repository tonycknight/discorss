namespace Discorss.Indexing

open System
open System.Text

module Tokenisation=

    let WordDelim = " .,!?[]<>()-".ToCharArray()

    let wordSplit (text: string) =
        
        seq {
            let mutable i = 0
            
            for j in [ 0 .. text.Length-1] do
                let c = text.[j]
                if Array.contains c WordDelim && (j - i > 1) then
                    yield text.Substring(i, j - i)
                    i <- j

            if (text.Length - i > 1) then
                yield text.Substring(i, text.Length - i)
        }

    let stripPunctuation (text: string) =        
        let result =    text
                        |> Seq.filter (Char.IsPunctuation >> not) 
                        |> Seq.fold (fun (sb: StringBuilder) c -> sb.Append(c) ) (new StringBuilder())
        
        result.ToString()
            