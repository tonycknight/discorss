namespace Discorss.Feeds.Test.Unit

open System
open Discorss
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit

module HtmlTests=

    [<Xunit.Theory>]
    [<Xunit.InlineData("", "")>]
    [<Xunit.InlineData("yadda", "yadda")>]
    [<Xunit.InlineData("<body>ya<a href=\"link\">dd</a>ah</body>", "yaddah")>]
    [<Xunit.InlineData("<body><table><th>test</th></table>yadda</body>", "testyadda")>]
    [<Xunit.InlineData("<body><script>do nothing</script>yadda</body>", "yadda")>]
    let ``stripHtml returns inner text`` html expected =
        html 
            |> Discorss.Html.stripHtml        
            |> Option.map id
            |> Option.defaultValue ""
            |> should equal expected
