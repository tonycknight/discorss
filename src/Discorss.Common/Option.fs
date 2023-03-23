namespace Discorss

open System

module Option =
    let ofNull (value: 'a) =
        if value = null then None else Some value
