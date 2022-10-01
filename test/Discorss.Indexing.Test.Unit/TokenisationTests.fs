namespace Discorss.Indexing.Tests.Unit

open System
open Discorss.Indexing
open FsUnit
open Xunit

module TokenisationTests=
    
    [<Fact>]
    let ``wordSplit of empty string is empty``()=
        let xs = "" |> Tokenisation.wordSplit |> Array.ofSeq

        xs |> should haveLength 0


    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(6)>]
    let ``wordSplit of equal size``(count)=
        let xs = "aaa" |> Seq.replicate count |> Array.ofSeq
        
        let text = xs |> Strings.join " "
        
        let words = text |> Tokenisation.wordSplit |> Array.ofSeq 
        
        let result = words |> Strings.join " "

        result |> should equal text


    [<Theory>]
    [<InlineData(" ", "")>]
    [<InlineData("a b", "a|b")>]
    [<InlineData("a  b", "a|b")>]
    [<InlineData(" a  b ", "a|b")>]
    [<InlineData(" a  b c", "a|b|c")>]
    let ``wordSplit of arbitrary texts``(text, expected: string)=
        
        let expectedWords = expected.Split('|', StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)
                                    |> Strings.join " "

        let result = text |> Tokenisation.wordSplit |> Strings.join " "

        result |> should equal expectedWords


    [<Theory>]
    [<InlineData("", "")>]
    [<InlineData(" ", "")>]
    [<InlineData("?", "")>]
    [<InlineData("<>", "<>")>]
    [<InlineData(" a ", "a")>]
    [<InlineData(" a! ", "a")>]
    [<InlineData(" a!A ", "aA")>]
    let ``stripPunctuation has punctuation and whitespace removed``(text, expected)=
        let result = text |> Tokenisation.stripPunctuation
        result |> should equal expected