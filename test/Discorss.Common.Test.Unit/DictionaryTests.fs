namespace Discorss.Common.Test.Unit

open System
open Discorss
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit

module DictionaryTests =

    [<Property>]
    let ``ofMap / fromMap is symmetric`` (value: Map<string, string>) =
        let result = value |> Dictionary.ofMap |> Dictionary.fromMap
        result |> should equalSeq value
        true