namespace Discorss

open Discorss.ApiModels
open Spectre.Console
open Spectre.Console.Cli

type DocumentsCommandSettings() =
    inherit BaseCommandSettings()

module DocumentsConsole =
    let private render value =
        value |> Console.markup |> Console.renderable

    let private yellow = Console.yellow
    let private cyan = Console.cyan

    let documentsLayout () =
        let titlePanel = Layout("title").Size(1)
        let uriPanel = Layout("uri").Size(1)
        let pubPanel = Layout("pub").Size(1)
        let categoriesPanel = Layout("categories").Size(1)
        let contentPanel = Layout("content").MinimumSize(1)
        let layoutRows = [| titlePanel; uriPanel; pubPanel; categoriesPanel; contentPanel |]
        Layout().SplitRows(layoutRows)

    let screenLayout () =
        let status =
            let panel = Panel("").Border(BoxBorder.Square).BorderColor(Color.Red)
            Layout("status").Update(panel).Size(2)

        let instructions =
            let line =
                seq {
                    yellow "Press "
                    cyan "Q"
                    yellow " to quit, "
                    cyan "O"
                    yellow " to open page, "
                    cyan "↑"
                    yellow " to like, "
                    cyan "↓"
                    yellow " to dislike, "
                    cyan "Del"
                    yellow " to remove like/dislike,"
                    yellow " any other key to continue."
                }
                |> Strings.join ""

            Panel(render line).Border(BoxBorder.Rounded).BorderColor(Color.Lime)

        let instructions = Layout(instructions).Size(4)

        Layout().SplitRows([| documentsLayout (); status; instructions |])


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

    let updateDocumentsLayout (layout: Layout) (document: Document option) =

        let title (doc: Document option) =
            doc
            |> Option.map (fun d -> d.title |> Strings.escapeMarkup |> Console.cyan)
            |> Option.defaultValue ""
            |> render

        let pub (doc: Document option) =
            match doc with
            | None -> ""
            | Some doc ->
                let cats =
                    seq {
                        doc.publication.ToString() |> Console.lightgrey
                        doc.author |> Strings.escapeMarkup |> Console.lightgrey
                    }
                    |> Seq.filter (fun x -> x <> "")
                    |> Strings.join " by "
                    |> Console.grey
                    |> Console.italic

                (Console.grey "Published: ") + cats
            |> render

        let uri (doc: Document option) =
            doc
            |> Option.map (fun d -> d.uri |> Strings.escapeMarkup |> Console.yellow)
            |> Option.defaultValue ""
            |> render

        let content (doc: Document option) =
            match doc with
            | None -> ""
            | Some doc ->
                seq {
                    (if doc.description.Length > 0 then doc.description else "")
                    |> Strings.escapeMarkup
                    |> Console.lightcyan

                    (if doc.content.Length > 0 then doc.content else "") |> Strings.escapeMarkup
                }
                |> Seq.filter (fun x -> x.Length > 0)
                |> Strings.join System.Environment.NewLine
            |> render

        let categories (doc: Document option) =
            let categories =
                doc
                |> Option.map _.categories
                |> Option.defaultValue [||]
                |> Seq.map (fun s -> s |> Console.lightgrey |> Console.italic)
                |> Strings.join ", "

            if categories.Length = 0 then
                ""
            else
                (Console.grey "Categories: ") + categories
            |> render

        layout.["title"].Update(title document) |> ignore
        layout.["uri"].Update(uri document) |> ignore
        layout.["pub"].Update(pub document) |> ignore
        layout.["categories"].Update(categories document) |> ignore
        layout.["content"].Update(content document) |> ignore

    let updateStatus (layout: Layout) (message: string) =
        layout.["status"].Update(message |> render) |> ignore

    let setFetchingStatus (layout: Layout) =
        "Fetching..." |> Console.cyan |> Console.italic |> updateStatus layout

    let updateDocumentFetchStatus (layout: Layout) document =
        match document with
        | None ->
            ("No article found." |> Console.red)
            + (" Hit Enter to try again." |> Console.cyan)
        | Some _ -> ""
        |> updateStatus layout

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

type ConsoleKey = System.ConsoleKey

type CycleDocumentsCommand() =
    inherit AsyncCommand<DocumentsCommandSettings>()

    let mainLayout = DocumentsConsole.screenLayout ()

    let likeDoc host (ctx: LiveDisplayContext) (value: bool) (document: Document) =
        let likeDoc host value (document: Document) =
            task {
                try
                    let! r = document |> DiscorssApi.likeDocument host value

                    return
                        if r.liked then
                            $"Document liked." |> Console.green
                        else
                            "Document unliked." |> Console.orange
                with ex ->
                    return ex.Message |> Console.red
            }

        task {
            let! msg = document |> likeDoc host value
            msg |> DocumentsConsole.updateStatus mainLayout
            ctx.UpdateTarget(mainLayout)
        }

    let deleteLikeDoc host (ctx: LiveDisplayContext) document =
        let deleteLikeDoc host (document: Document) =
            task {
                try
                    do! document |> DiscorssApi.deleteDocumentLike host
                    return "Document like removed." |> Console.orange
                with ex ->
                    return ex.Message |> Console.red
            }

        task {
            let! msg = document |> deleteLikeDoc host
            msg |> DocumentsConsole.updateStatus mainLayout
            ctx.UpdateTarget(mainLayout)
        }

    let openBrowser (document: Document) =
        document.uri |> Process.openUri |> ignore

    let getNextDocument (settings: DocumentsCommandSettings) (ctx: LiveDisplayContext) =
        task {
            try
                DocumentsConsole.setFetchingStatus mainLayout
                ctx.UpdateTarget(mainLayout)

                let! doc = DiscorssApi.nextDocument settings.ApiHost
                
                doc |> DocumentsConsole.updateDocumentsLayout mainLayout
                doc |> DocumentsConsole.updateDocumentFetchStatus mainLayout
                (*
                let! like =
                    task {
                        match doc with
                        | None -> return None
                        | Some d ->
                            let! result = d |> DiscorssApi.getLikeDocument settings.ApiHost
                            return Some result
                    }
                let msg = like |> Option.map (fun l -> if l.liked then "You liked this document." else "You disliked this document.") |> Option.defaultValue ""
                msg |> DocumentsConsole.updateStatus mainLayout
                *)
                ctx.UpdateTarget(mainLayout)

                return doc
            with ex ->
                None |> DocumentsConsole.updateDocumentsLayout mainLayout
                ex.Message |> Console.red |> DocumentsConsole.updateStatus mainLayout
                ctx.UpdateTarget(mainLayout)
                return None
        }

    let keyLoop (settings: DocumentsCommandSettings) (ctx: LiveDisplayContext) (doc: Document option) =
        task {
            let mutable quitApp = false
            let mutable quitLoop = false
            let likeDoc = likeDoc settings.ApiHost ctx

            while not quitLoop do
                match System.Console.ReadKey(true).Key with
                | ConsoleKey.Q ->
                    quitApp <- true
                    quitLoop <- true
                | ConsoleKey.UpArrow
                | ConsoleKey.Add
                | ConsoleKey.OemPlus ->
                    match doc with
                    | Some doc -> do! doc |> likeDoc true
                    | None -> ignore 0
                | ConsoleKey.DownArrow
                | ConsoleKey.Subtract
                | ConsoleKey.OemMinus ->
                    match doc with
                    | Some doc -> do! doc |> likeDoc false
                    | None -> ignore 0
                | ConsoleKey.Delete ->
                    match doc with
                    | Some doc -> do! doc |> deleteLikeDoc settings.ApiHost ctx
                    | None -> ignore 0
                | ConsoleKey.O -> doc |> Option.iter openBrowser
                | _ -> quitLoop <- true

            return quitApp
        }

    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            do!
                AnsiConsole
                    .Live(mainLayout)
                    .AutoClear(true)
                    .StartAsync(fun ctx ->
                        task {
                            let mutable quit = false

                            None |> DocumentsConsole.updateDocumentsLayout mainLayout

                            while not quit do

                                let! doc = getNextDocument settings ctx

                                let! q = keyLoop settings ctx doc
                                quit <- q
                        })

            return ReturnCodes.ok
        }

    interface ICommandLimiter<CommandSettings>
