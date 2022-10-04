namespace Discorss.Feeds.Test.Unit

open System
open Discorss
open FsCheck
open FsCheck.Xunit
open FsUnit

module CombinatorsTests=

    [<Xunit.Theory>]
    [<Xunit.InlineData(true, true, true)>]
    [<Xunit.InlineData(false, true, false)>]
    [<Xunit.InlineData(true, false, false)>]
    let ``>&&> ``(left, right, expected)=
        let a = fun (x) -> left
        let b = fun (x) -> right

        let f = a >&&> b

        let result = f 1

        result |> should equal expected
