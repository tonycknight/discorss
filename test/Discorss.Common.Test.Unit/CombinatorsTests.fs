namespace Discorss.Common.Test.Unit

open Discorss
open FsCheck
open FsUnit.Xunit

module CombinatorsTests =

    [<Xunit.Theory>]
    [<Xunit.InlineData(true, true, true)>]
    [<Xunit.InlineData(false, true, false)>]
    [<Xunit.InlineData(true, false, false)>]
    [<Xunit.InlineData(false, false, false)>]
    let ``&&>> is applied`` (left, right, expected) =
        let a = fun (x) -> left
        let b = fun (x) -> right

        let f = a &&>> b

        let result = f 1

        result |> should equal expected

    [<Xunit.Theory>]
    [<Xunit.InlineData(true, true, true)>]
    [<Xunit.InlineData(false, true, true)>]
    [<Xunit.InlineData(true, false, true)>]
    [<Xunit.InlineData(false, false, false)>]
    let ``||>> is applied`` (left, right, expected) =
        let a = fun (x) -> left
        let b = fun (x) -> right

        let f = a ||>> b

        let result = f 1

        result |> should equal expected
