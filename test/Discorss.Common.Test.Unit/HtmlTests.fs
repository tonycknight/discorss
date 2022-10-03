namespace Discorss.Feeds.Test.Unit

open System
open Discorss
open FsCheck
open FsCheck.Xunit
open FsUnit

module HtmlTests=

    [<Xunit.Theory>]
    [<Xunit.InlineData("<body><table><th>test</th></table>yadda</body>", "yadda")>]
    [<Xunit.InlineData("<body><script>do nothing</script>yadda</body>", "yadda")>]
    let ``stripHtml returns inner text`` html expected =
        let txt = Discorss.Html.stripHtml html
        
        txt |> Option.map id
            |> Option.defaultValue ""
            |> should equal expected
