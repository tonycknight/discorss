namespace Discorss.Common.Test.Unit

open System
open Discorss
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit

module DictionaryTests =

    [<Property>]
    let ``ofMap / toMap is symmetric`` (value: Map<string, string>) =
        let result = value |> Dictionary.ofMap |> Dictionary.toMap
        result |> should equalSeq value
        true
