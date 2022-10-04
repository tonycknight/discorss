namespace Discorss

open System

module Option=
    let ofNull(value: 'a)=
        if value = null then None
        else Some value

module Seq=
    let noneToEmpty(xs: seq<'a> option) =
        match xs with
        | Some xs -> xs
        | _ -> Seq.empty<'a>        

