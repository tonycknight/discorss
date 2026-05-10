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
                        
            c.AddBranch<CommandSettings>("feeds", 
                        fun c ->   c.AddCommand<AddFeedCommand>("add").WithDescription("Add a new feed") |> ignore
                                   c.AddCommand<ListFeedsCommand>("list").WithDescription("List feeds") |> ignore
                                   ) |> ignore

            (*            
            c.AddBranch("documents",
                        fun c1 -> ignore c1
                        ) |> ignore
            *)
            )

        try
            app.Run(args)
        with ex ->
            ex.Message |> Strings.escapeMarkup |> Console.error |> console
            ReturnCodes.sysError