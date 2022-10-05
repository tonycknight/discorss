namespace Discorss.Indexing

open System
open System.Text
open Discorss

module Tokenisation=

    let private WordDelim = " ,;!?[]<>(){}-\r\n\b\t\\".ToCharArray()

    let wordSplit (text: string) =
        
        seq {
            let mutable i = 0
            for j = 0 to text.Length-1 do
                let c = text.[j]
                // TODO: future: consider Char.IsLetterOrDigit >> not
                if Array.contains c WordDelim then
                    let k = j - i
                    if k > 0 then
                        yield text.Substring(i, k).Trim()
                    i <- j+1
            let k = text.Length - i
            if k > 0 then
                yield text.Substring(i, k).Trim()
        } |> Seq.filter (fun s -> s.Length > 0)

    let stripPunctuation (text: string) =                
        let result =    text
                        |> Seq.filter (Char.IsPunctuation >> not) 
                        |> Seq.filter (Char.IsWhiteSpace >> not) 
                        |> Seq.fold (fun (sb: StringBuilder) c -> sb.Append(c) ) (new StringBuilder())        
        result.ToString()
    
    let stripTrailingPunctuation (text: string) =                 
        let mutable result = ""
        let mutable i = (text.Length - 1)

        while i >= 0 && result = "" do        
            let c = text.[i]
            if (Char.IsPunctuation c || Char.IsWhiteSpace c) |> not then
                result <- text.Substring(0, i + 1).Trim()
            else
                i <- i - 1
        result
             

    let wordify (lexicon: ILexicon)  (text: string)=
        let stripPunctuation word = 
            let word2 = stripTrailingPunctuation word
            match lexicon.IsKnownWord word2 with 
            | true -> word2
            | _ -> stripPunctuation word

        text    |> Option.ofNull
                |> Option.map (wordSplit >> (Seq.map stripPunctuation ))
                |> Option.defaultValue Seq.empty