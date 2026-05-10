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

    override this.Validate() : ValidationResult =
        if Strings.isEmptyWhitespace this.FeedUri then
            ValidationResult.Error "The Feed URI is missing."
        else if Strings.isUri this.FeedUri |> not then
            ValidationResult.Error "The Feed URI is invalid."
        else
            base.Validate()

module FeedsConsole =

    let feedTable (feeds: ApiModels.FeedInfo seq) =
        let feedRows (feed: ApiModels.FeedInfo) =
            seq {
                $"{feed.title |> Console.cyan} {feed.uri |> Console.yellow}"
                $"  {feed.description |> Console.white}"
                $"  Last fetched on {feed.lastFetched.ToString()}" |> Console.italic
            }
            |> Seq.map (Console.markup >> Console.renderable)

        let table = Console.table () |> Console.tableColumn ""

        let addFeed feed table =
            let rows = feedRows feed
            rows |> Seq.fold (fun t r -> Console.tableRow t [| r |]) table

        feeds |> Seq.fold (fun t e -> addFeed e t) table

type ListFeedsCommand(nuget: Tk.Nuget.INugetClient) =
    inherit AsyncCommand<ListFeedsCommandSettings>()

    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            if not settings.NoBanner then
                Commands.renderBanner nuget

            let! feeds = DiscorssApi.getFeeds settings.ApiHost

            FeedsConsole.feedTable feeds |> AnsiConsole.Console.Write

            return 0
        }

    interface ICommandLimiter<CommandSettings>


type AddFeedCommand(nuget: Tk.Nuget.INugetClient) =
    inherit AsyncCommand<AddFeedCommandSettings>()

    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            if not settings.NoBanner then
                Commands.renderBanner nuget

            return 0
        }

    interface ICommandLimiter<CommandSettings>
