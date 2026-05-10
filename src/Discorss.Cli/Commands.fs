namespace Discorss

open System.ComponentModel
open Spectre.Console
open Spectre.Console.Cli

type BaseCommandSettings() =
    inherit CommandSettings()
        
    [<CommandOption("-a|--api")>]
    [<Description("The Discorss server URI.")>]
    [<DefaultValue("http://localhost:8080")>]
    member val ApiHost = "" with get, set

    [<CommandOption("--trace")>]
    [<Description("Show detailed working and Nuget results.")>]
    [<DefaultValue(false)>]
    member val TraceLogging = false with get, set

    [<CommandOption("--no-banner")>]
    [<Description("Don't show the banner.")>]
    [<DefaultValue(false)>]
    member val NoBanner = false with get, set

    override this.Validate (): ValidationResult =         
        if Strings.isEmptyWhitespace this.ApiHost then ValidationResult.Error "The API URI is missing."
        else if Strings.isUri this.ApiHost |> not then ValidationResult.Error "The API URI is invalid."
        else base.Validate()