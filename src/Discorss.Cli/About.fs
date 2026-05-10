namespace Discorss

open Discorss.ApiModels
open Spectre.Console
open Spectre.Console.Cli

type AboutCommandSettings() =
    inherit BaseCommandSettings()

module AboutConsole =

    let about (host, heartbeat, stats: ApiModels.Stats seq) =
        let statRow (stats: ApiModels.Stats) =
            [| stats.name; stats.itemCount.ToString() |> Console.cyan |]
            |> Array.map (Console.markup >> Console.renderable)

        let header =
            [| "Host"
               seq {
                   Console.cyan host

                   if heartbeat then
                       Console.green ":check_mark_button: Server is OK"
                   else
                       Console.red ":warning:  Server is sick"
               }
               |> Strings.join " " |]
            |> Array.map (Console.markup >> Console.renderable)

        let table = Console.table () |> Console.tableColumn "" |> Console.tableColumn ""

        let rows = stats |> Seq.map statRow |> Seq.append [| header |]

        rows |> Seq.fold (fun t r -> r |> Console.tableRow t) table


type AboutCommand(nuget: Tk.Nuget.INugetClient) =
    inherit AsyncCommand<AboutCommandSettings>()

    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            if not settings.NoBanner then
                Commands.renderBanner nuget

            let! heartbeat = DiscorssApi.getHeartbeat settings.ApiHost
            let! stats = Exception.catchDefault [||] (fun () -> DiscorssApi.getStats settings.ApiHost)

            AboutConsole.about (settings.ApiHost, heartbeat, stats)
            |> AnsiConsole.Console.Write

            return if heartbeat then ReturnCodes.ok else ReturnCodes.sysError
        }

    interface ICommandLimiter<CommandSettings>
