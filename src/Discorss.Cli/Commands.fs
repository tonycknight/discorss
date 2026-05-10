namespace Discorss

open System.ComponentModel
open Spectre.Console
open Spectre.Console.Cli

module Commands =
    
    let validate predicate (msg: string) (value: 'a) =
        match predicate value with
        | true -> Choice1Of2 value
        | false -> ValidationResult.Error msg |> Choice2Of2

    let isNotEmptyWhitespace (msg: string) = validate Strings.isEmptyWhitespace msg 

    let isUri (msg: string) = validate Strings.isUri msg

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
        
        if Strings.isEmptyWhitespace this.ApiHost |> not then
            if Strings.isUri this.ApiHost |> not then
                ValidationResult.Error "Invalid API URI."     
            else
                base.Validate()
        else
            base.Validate()