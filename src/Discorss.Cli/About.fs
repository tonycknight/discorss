namespace Discorss

open System
open System.ComponentModel
open Discorss.ApiModels
open Spectre.Console
open Spectre.Console.Cli

type AboutCommandSettings() =
    inherit BaseCommandSettings()

module AboutConsole = 
    let x = 0

type AboutCommand(nuget: Tk.Nuget.INugetClient) =
    inherit AsyncCommand<AboutCommandSettings>()

    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            if not settings.NoBanner then
                Commands.renderBanner nuget

            // TODO:

            return ReturnCodes.ok
        }

    interface ICommandLimiter<CommandSettings>