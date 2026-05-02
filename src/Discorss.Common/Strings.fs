namespace Discorss

open System
open System.Security.Cryptography

module Strings =
    let str (value: 'a) = value.ToString()

    let join (delim: string) (strings: seq<string>) = String.Join(delim, strings)

    let lower (text: string) = text.ToLower()

    let upper (text: string) = text.ToUpper()

    let mixed (text: string) =
        let flip =
            let rng = new Random()
            fun () -> rng.Next(2) = 0

        text
        |> Seq.map (fun c ->
            match flip () with
            | true -> Char.ToUpper(c)
            | _ -> Char.ToLower(c))
        |> Array.ofSeq
        |> String

    let sha256 (value: string) =
        use x = SHA256.Create()
        let b = System.Text.Encoding.UTF8.GetBytes value

        let hash = x.ComputeHash(b)

        System.Convert.ToBase64String hash
        
    let sha512 (value: string) =
        use x = SHA512.Create()
        let b = System.Text.Encoding.UTF8.GetBytes value

        let hash = x.ComputeHash(b)

        System.Convert.ToBase64String hash
        