namespace Discorss

open System
open System.Diagnostics.CodeAnalysis

module ReturnCodes =

    [<Literal>]
    let validationOk = 0

    [<Literal>]
    let validationFailed = 1

    [<Literal>]
    let sysError = 2

module Strings =

    let isEmptyWhitespace (value: string) = String.IsNullOrWhiteSpace value

    let isUri (value: string) = Uri.TryCreate(value, UriKind.Absolute) |> fst
        
    let join (delim: string) (strings: seq<string>) = String.Join(delim, strings)

    let escapeMarkup (value: string) = value.Replace("[", "[[").Replace("]", "]]")

module Environment =

    [<ExcludeFromCodeCoverage>]
    let isRunningGithub =
        System.Environment.GetEnvironmentVariable("GITHUB_ACTIONS") <> null