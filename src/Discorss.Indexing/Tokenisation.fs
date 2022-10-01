namespace Discorss.Indexing

open System
open System.Text

module Tokenisation=

    let private WordDelim = " .,!?[]<>()-".ToCharArray()

    let wordSplit (text: string) =
        
        seq {
            let mutable i = 0
            
            for j in [ 0 .. text.Length-1] do
                let c = text.[j]
                if Array.contains c WordDelim then
                    yield text.Substring(i, j - i).Trim()
                    i <- j+1

            yield text.Substring(i, text.Length - i).Trim()
        } |> Seq.filter (fun s -> s.Length > 0)

    let stripPunctuation (text: string) =        
        let result =    text
                        |> Seq.filter (Char.IsPunctuation >> not) 
                        |> Seq.filter (Char.IsWhiteSpace >> not) 
                        |> Seq.fold (fun (sb: StringBuilder) c -> sb.Append(c) ) (new StringBuilder())
        
        result.ToString()
            