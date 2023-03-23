namespace Discorss.Indexing.Tests.Unit

open System
open Discorss
open Discorss.Indexing
open FsUnit
open Xunit

module TokenisationTests =

    [<Fact>]
    let ``wordSplit of empty string is empty`` () =
        "" |> Tokenisation.wordSplit |> Array.ofSeq |> should haveLength 0


    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(6)>]
    let ``wordSplit of equal size`` (count) =
        let xs = "aaa" |> Seq.replicate count |> Array.ofSeq

        let text = xs |> Strings.join " "

        let words = text |> Tokenisation.wordSplit |> Array.ofSeq

        words |> Strings.join " " |> should equal text


    [<Theory>]
    [<InlineData("", "")>]
    [<InlineData(" ", "")>]
    [<InlineData("a b", "a|b")>]
    [<InlineData("a  b", "a|b")>]
    [<InlineData(" a  b ", "a|b")>]
    [<InlineData(" a  b c", "a|b|c")>]
    [<InlineData(" a  b    c", "a|b|c")>]
    [<InlineData(" a\nb\nc", "a|b|c")>]
    [<InlineData(" a\nb", "a|b")>]
    let ``wordSplit of arbitrary texts`` (text, expected: string) =

        let expectedWords =
            expected.Split('|', StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)
            |> Strings.join " "

        text |> Tokenisation.wordSplit |> Strings.join " " |> should equal expectedWords


    [<Theory>]
    [<InlineData("", "")>]
    [<InlineData(" ", "")>]
    [<InlineData("?", "")>]
    [<InlineData("<>", "<>")>]
    [<InlineData(" a.", "a")>]
    [<InlineData(" .Net", "Net")>]
    [<InlineData(" a! ", "a")>]
    [<InlineData(" a!A ", "aA")>]
    let ``stripPunctuation has punctuation and whitespace removed`` (text, expected) =
        text |> Tokenisation.stripPunctuation |> should equal expected

    [<Theory>]
    [<InlineData("", "")>]
    [<InlineData(" ", "")>]
    [<InlineData("?", "")>]
    [<InlineData("<>", "<>")>]
    [<InlineData(" a. ", "a")>]
    [<InlineData(" .Net", ".Net")>]
    [<InlineData(" .Net!.", ".Net")>]
    [<InlineData(" a!A ", "a!A")>]
    let ``stripTrailingPunctuation has punctuation and whitespace removed`` (text, expected) =
        text |> Tokenisation.stripTrailingPunctuation |> should equal expected

    [<Theory>]
    [<InlineData("", false)>]
    [<InlineData(" ", false)>]
    [<InlineData(" a ", true)>]
    [<InlineData(" a1 ", true)>]
    [<InlineData(" =a", true)>]
    [<InlineData(" =", false)>]
    [<InlineData("=", false)>]
    [<InlineData("=a", true)>]
    let ``isCandidateWord`` (text, expected) =
        text |> Tokenisation.isCandidateWord |> should equal expected
