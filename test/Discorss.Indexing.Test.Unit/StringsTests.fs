namespace Discorss.Indexing.Tests.Unit

open System
open Discorss.Indexing
open FsCheck
open FsCheck.Xunit
open FsUnit

module StringsTests=
    
    [<Xunit.Fact>]
    let ``join on empty``() =
        let value = [ ] |> Strings.join " "
        value |> should equal ""


    [<Property(Verbose = true)>]
    let ``join on multiple lines``(NonEmptyString value, PositiveInt count)=
        let values = [1 .. count] |> Seq.map (fun _ -> value)
        let expected = String.Join(' ', values)

       
        let result = values |> Strings.join " "

        result = expected
    

