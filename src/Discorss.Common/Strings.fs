namespace Discorss

open System

module Strings=
    let str (value: 'a) = value.ToString()

    let join (delim : string) (strings: seq<string>) = String.Join(delim, strings)
    
    let lower (text: string) = text.ToLower()

    let upper (text: string) = text.ToUpper()

    let mixed (text: string) = 
        let flip = 
            let rng = new Random()
            fun () -> rng.Next(2) = 0
        
        text |> Seq.map (fun c -> match flip() with
                                    | true -> Char.ToUpper(c)
                                    | _ -> Char.ToLower(c)
                        )
            |> Array.ofSeq
            |> String

