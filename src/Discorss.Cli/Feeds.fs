namespace Discorss

open System
open System.ComponentModel
open Discorss.ApiModels
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
    let feedPreview (feed: ApiModels.Feed) =
        let channel (feed: ApiModels.Feed) =
            seq {
                $"{feed.feed.title |> Strings.escapeMarkup |> Console.cyan} {feed.feed.uri |> Console.yellow}"
                $"{feed.feed.description |> Strings.escapeMarkup |> Console.white}"
            }

        let entry (entry: ApiModels.FeedEntry) =
            seq {
                $"{entry.title |> Strings.escapeMarkup |> Console.cyan}"
                $"{entry.content |> Strings.truncate 100 |> Strings.escapeMarkup |> Console.white}"
            }
            |> Strings.join Environment.NewLine

        let rows (feed: ApiModels.Feed) =
            seq {
                yield! channel feed
                yield! feed.entries |> Seq.map entry
            }
            |> Seq.map (Console.markup >> Console.renderable)

        let table = Console.table () |> Console.tableColumn ""

        feed |> rows |> Seq.fold (fun t r -> Console.tableRow t [| r |]) table

    let feedsTable (feeds: ApiModels.FeedInfo seq) =
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

            feeds |> FeedsConsole.feedsTable |> AnsiConsole.Console.Write

            return ReturnCodes.ok
        }

    interface ICommandLimiter<CommandSettings>

type PreviewFeedCommand(nuget: Tk.Nuget.INugetClient) =
    inherit AsyncCommand<AddFeedCommandSettings>()

    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            if not settings.NoBanner then
                Commands.renderBanner nuget

            let! feed = DiscorssApi.previewFeeds settings.ApiHost settings.FeedUri

            feed |> FeedsConsole.feedPreview |> AnsiConsole.Console.Write

            return ReturnCodes.ok
        }

    interface ICommandLimiter<CommandSettings>


type AddFeedCommand(nuget: Tk.Nuget.INugetClient) =
    inherit AsyncCommand<AddFeedCommandSettings>()

    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            if not settings.NoBanner then
                Commands.renderBanner nuget

            let feed =
                { FeedInfo.uri = settings.FeedUri
                  title = ""
                  description = ""
                  updated = DateTime.UtcNow
                  lastFetched = DateTime.UtcNow }

            let! feed = DiscorssApi.addFeeds settings.ApiHost feed

            [ feed ] |> FeedsConsole.feedsTable |> AnsiConsole.Console.Write

            return ReturnCodes.ok
        }

    interface ICommandLimiter<CommandSettings>
