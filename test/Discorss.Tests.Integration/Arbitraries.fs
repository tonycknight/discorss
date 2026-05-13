namespace Discorss.Tests.Integration

open System
open Discorss
open FsCheck
open FsCheck.FSharp

[<AutoOpen>]
module Arbitraries =
    let isNotNullOrEmpty = String.IsNullOrEmpty >> not

    let isAlphaNumeric (value: string) =
        value |> Seq.forall (Char.IsLetter ||>> Char.IsNumber)

type AlphaNumericString =

    static member Generate() =
        ArbMap.defaults
        |> ArbMap.arbitrary<string>
        |> Arb.filter (isNotNullOrEmpty &&>> isAlphaNumeric)