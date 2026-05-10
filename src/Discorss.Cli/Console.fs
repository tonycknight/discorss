namespace Discorss

open System
open Spectre.Console

module Console =
    [<Literal>]
    let private nugetPrefix = "https://www.nuget.org/packages"

    let console = Spectre.Console.AnsiConsole.MarkupLine

    let markup x = new Markup(x)

    let renderable x =
        x :> Spectre.Console.Rendering.IRenderable

    let style (style: string) (value: string) = $"[{style}]{value}[/]"
    let italic = style "italic"
    let white = style "white"
    let green = style "lime"
    let cyan = style "cyan"
    let lightcyan = style "#d7ffff"
    let darkcyan = style "#00af87"
    let yellow = style "yellow"
    let orange = style "#f57a51"
    let blue = style "#6495ed"
    let error = style "red"
    let lightgrey = style "#A0A0A0"

    let grey =
        match Environment.isRunningGithub with
        | true -> style "#A0A0A0"
        | _ -> style "grey"

    let table () =
        let table = new Table()
        table.Border <- TableBorder.None
        table.ShowHeaders <- false
        table

    let tableColumn (name: string) (table: Table) = table.AddColumn(name)

    let tableRow (table: Table) (row: Rendering.IRenderable array) =
        table.Rows.Add row |> ignore
        table
