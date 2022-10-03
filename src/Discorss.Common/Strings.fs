namespace Discorss

open System

module Strings=
    let join (delim : string) (strings: seq<string>) = String.Join(delim, strings)
    

