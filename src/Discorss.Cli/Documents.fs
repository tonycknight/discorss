namespace Discorss

open Discorss.ApiModels
open Spectre.Console
open Spectre.Console.Cli

type DocumentsCommandSettings() =
    inherit BaseCommandSettings()

module DocumentsConsole =
    let document (document: ApiModels.Document) =
        let cats = document.categories |> Strings.join ", "

        seq {
            document.title |> Strings.escapeMarkup |> Console.cyan
            document.uri |> Console.yellow |> Console.italic

            $"{document.publication.ToString()} - {document.author}"
            |> Console.grey
            |> Console.italic

            if document.description.Length > 0 then
                document.description |> Strings.escapeMarkup
            else
                document.content |> Strings.escapeMarkup

            if cats.Length > 0 then
                $"Categories: {cats |> Console.italic}" |> Console.grey
        }
        |> Strings.join System.Environment.NewLine
        |> Console.markup
        |> Console.renderable


type GetNextDocumentCommand(nuget: Tk.Nuget.INugetClient) =
    inherit AsyncCommand<DocumentsCommandSettings>()

    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            if not settings.NoBanner then
                Commands.renderBanner nuget

            let! r = DiscorssApi.nextDocument settings.ApiHost

            match r with
            | None -> AnsiConsole.Console.Write("Not found.")
            | Some doc -> doc |> DocumentsConsole.document |> AnsiConsole.Console.Write

            return ReturnCodes.ok
        }

    interface ICommandLimiter<CommandSettings>
