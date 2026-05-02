namespace Discorss.Feeds.Test.Unit

open System
open Discorss
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit

module StringsTests =

    [<Property(Verbose = true)>]
    let ``join yields concatenation`` (NonEmptyString value, PositiveInt count) =
        let xs = [ 1..count ] |> List.map (fun _ -> value)

        let result = xs |> Strings.join " "
        let expected = System.String.Join(' ', xs)

        result |> should equal expected

    [<Property(Verbose = true)>]
    let ``lower yields lower case`` (NonEmptyString value) =

        let result = value |> Strings.lower

        value.ToLower() |> should equal result

    [<Property(Verbose = true)>]
    let ``upper yields upper case`` (NonEmptyString value) =

        let result = value |> Strings.upper

        value.ToUpper() |> should equal result

    [<Property(Verbose = true)>]
    let ``sha256 is idempotent`` (NonEmptyString value) =
        Strings.sha256 value = Strings.sha256 value

    [<Property(Verbose = true)>]
    let ``sha256 produces non-empty string`` (NonEmptyString value) =
        Strings.sha256 value |> String.IsNullOrWhiteSpace |> not

    [<Property(Verbose = true)>]
    let ``sha256 produces base64 string`` (NonEmptyString value) =
        Strings.sha256 value |> System.Convert.FromBase64String |> (<>) Array.empty
