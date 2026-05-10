namespace Discorss.Html.Test.Unit

open System
open Discorss
open FsCheck
open FsUnit.Xunit

module HtmlTests =

    [<Xunit.Theory>]
    [<Xunit.InlineData("", "")>]
    [<Xunit.InlineData("yadda", "yadda")>]
    [<Xunit.InlineData("<body>ya<a href=\"link\">dd</a>ah</body>", "yaddah")>]
    [<Xunit.InlineData("<body><table><th>test</th></table>yadda</body>", "testyadda")>]
    [<Xunit.InlineData("<body><script>do nothing</script>yadda</body>", "yadda")>]
    [<Xunit.InlineData("<<", "<<")>]
    [<Xunit.InlineData("<aaa", "")>]
    [<Xunit.InlineData("aaa>", "aaa>")>]
    [<Xunit.InlineData("<<>", "<<>")>]
    let ``stripHtml returns inner text`` html expected =
        html
        |> Discorss.Html.stripHtml
        |> Option.defaultValue ""
        |> should equal expected

    [<Xunit.Theory>]
    [<Xunit.InlineData(null)>]
    let ``stripHtml returns None on error`` html =
        let r = html |> Discorss.Html.stripHtml

        r |> should equal None
