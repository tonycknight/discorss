namespace Discorss.Indexing

open System
open System.Text
open Discorss

module Tokenisation=

    let private WordDelim = " ,!?[]<>()-".ToCharArray()

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
    
    let stripTrailingPunctuation (text: string) =                
        let result =    text
                        |> Seq.rev
                        |> Seq.skipWhile (Char.IsPunctuation >||> Char.IsWhiteSpace)
                        |> Seq.rev
                        |> Seq.filter (Char.IsWhiteSpace >> not) 
                        |> Seq.fold (fun (sb: StringBuilder) c -> sb.Append(c) ) (new StringBuilder())        
        result.ToString()

    let wordify (lexicon: ILexicon)  (text: string)=
        let stripPunctuation word = 
            let word2 = stripTrailingPunctuation word
            match lexicon.IsKnownWord word2 with 
            | true -> word2
            | _ -> stripPunctuation word

        text    |> Option.ofNull
                |> Option.map (wordSplit >> (Seq.map stripPunctuation ))
                |> Option.defaultValue Seq.empty