namespace Discorss

open Spectre.Console.Cli

module Program =
    
    let console = Spectre.Console.AnsiConsole.MarkupLine

    [<EntryPoint>]
    let main args =
        System.Console.OutputEncoding <- System.Text.Encoding.UTF8

        let svcs = App.spectreServices ()
        let app = CommandApp(svcs)

        app.Configure(fun c ->
            c
                .SetApplicationName(App.packageId)
                .SetApplicationVersion(App.version () |> Option.defaultValue "")
                .PropagateExceptions()
                .UseStrictParsing()
                .ValidateExamples()
                .TrimTrailingPeriods(false)
            |> ignore
            
            // TODO: 

            )

        try
            app.Run(args)
        with ex ->
            ex.Message |> Strings.escapeMarkup |> Console.error |> console
            ReturnCodes.sysError