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

            c.AddBranch<CommandSettings>(
                "feeds",
                fun c ->
                    c.SetDescription("Work with feeds.")
                    c.AddCommand<AddFeedCommand>("add").WithDescription("Add a new feed.") |> ignore
                    c.AddCommand<ListFeedsCommand>("list").WithDescription("List feeds.") |> ignore

                    c.AddCommand<PreviewFeedCommand>("preview").WithDescription("Preview a feed.")
                    |> ignore
            )
            |> ignore

            c.AddBranch<CommandSettings>(
                "documents",
                fun c ->
                    c.SetDescription("Work with documents.")

                    c
                        .AddCommand<GetNextDocumentCommand>("next")
                        .WithDescription("Get the next document in your queue.")
                    |> ignore

                    c.AddCommand<CycleDocumentsCommand>("cycle")
                     .WithDescription("Cycle the documents in your queue.")
                     |> ignore
            )
            |> ignore

            c.AddCommand<AboutCommand>("about").WithDescription("Get information about the server.")
            |> ignore

        )

        try
            app.Run(args)
        with ex ->
            ex.Message |> Strings.escapeMarkup |> Console.error |> console
            ReturnCodes.sysError
