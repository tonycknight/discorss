namespace Discorss

open System

module Strings=
    let join (delim : string) (strings: seq<string>) = String.Join(delim, strings)
    
    let lower (text: string) = text.ToLower()

    let upper (text: string) = text.ToUpper()
