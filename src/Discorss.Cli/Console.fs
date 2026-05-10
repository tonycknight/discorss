namespace Discorss

open System
open Spectre.Console

module Console =
    [<Literal>]
    let private nugetPrefix = "https://www.nuget.org/packages"

    let toRenderable x = x :> Spectre.Console.Rendering.IRenderable

    let markup (style: string) (value: string) = $"[{style}]{value}[/]"
    let italic = markup "italic"
    let white = markup "white"
    let green = markup "lime"
    let cyan = markup "cyan"
    let lightcyan = markup "#d7ffff"
    let darkcyan = markup "#00af87"
    let yellow = markup "yellow"
    let orange = markup "#f57a51"
    let blue = markup "#6495ed"
    let error = markup "red"
    let lightgrey = markup "#A0A0A0"

    let grey =
        match Environment.isRunningGithub with
        | true -> markup "#A0A0A0"
        | _ -> markup "grey"

    let table () =
        let table = new Table()
        table.Border <- TableBorder.None
        table.ShowHeaders <- false
        table

    let tableColumn (name: string) (table: Table) = table.AddColumn(name)

    let nugetLinkPkgVsn package version =
        let url = $"{nugetPrefix}/{package}/{version}"
        $"[link={url}]{package} {version}[/]"

    let nugetLinkPkgVsnOnly package version =
        let url = $"{nugetPrefix}/{package}/{version}"
        $"[link={url}]{version}[/]"

    let nugetLinkPkgSuggestion package suggestion =
        let url = $"{nugetPrefix}/{package}"
        $"[link={url}]{package} {suggestion}[/]"

    