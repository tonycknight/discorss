namespace Discorss

open System.ComponentModel
open Spectre.Console
open Spectre.Console.Cli

type FeedsCommandSettings() =
    inherit BaseCommandSettings()

type ListFeedsCommandSettings() =
    inherit FeedsCommandSettings()

type AddFeedCommandSettings() =
    inherit FeedsCommandSettings()

    [<CommandArgument(0, "[FEED_URI]")>]
    [<Description("The feed URI.")>]
    member val FeedUri = "" with get, set
        
    override this.Validate (): ValidationResult = 
        if Strings.isEmptyWhitespace this.FeedUri then
            ValidationResult.Error "The Feed URI is missing."
        else if Strings.isUri this.FeedUri |> not then
            ValidationResult.Error "Invalid Feed URI."        
        else            
            base.Validate()

type ListFeedsCommand() =
    inherit AsyncCommand<ListFeedsCommandSettings>()

    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            return 0
        }

    interface ICommandLimiter<CommandSettings>
    

type AddFeedCommand() =
    inherit AsyncCommand<AddFeedCommandSettings>()

    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            return 0
        }

    interface ICommandLimiter<CommandSettings>
