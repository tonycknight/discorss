namespace Discorss

open Discorss.ApiModels
open Spectre.Console
open Spectre.Console.Cli

type DocumentsCommandSettings() =
    inherit BaseCommandSettings()

module DocumentsConsole =
    let private render value = value |> Console.markup |> Console.renderable

    let documentsLayout () =
        let titlePanel = Layout("title").Size(1)
        let uriPanel = Layout("uri").Size(1)
        let pubPanel = Layout("pub").Size(1)
        let contentPanel = Layout("content").MinimumSize(1)    
        let layoutRows = [| titlePanel; uriPanel; pubPanel; contentPanel |]    
        Layout().SplitRows(layoutRows)
        
    let screenLayout () =
        let instructions = 
            Panel("Press Q to quit, O to open page, any key to continue to next" |> Console.yellow |> render)
                .Border(BoxBorder.Rounded)        
                .BorderColor(Color.Lime)
        let instructions = Layout(instructions).Size(4)
        
        Layout().SplitRows( [| documentsLayout (); instructions |])
        

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
            doc |> Option.map (fun d -> d.title |> Strings.escapeMarkup |> Console.cyan) |> Option.defaultValue (Console.red "No document found.") |> render
                    
        let pub (doc: Document option) =
            match doc with
            | None -> ""
            | Some doc -> 
                seq { 
                    doc.publication.ToString()
                    doc.author |> Strings.escapeMarkup
                } |> Seq.filter (fun x -> x <> "") |> Strings.join " - "
                |> Console.grey |> Console.italic
            |> render
            
        let uri (doc: Document option) =
            doc |> Option.map (fun d -> d.uri |> Strings.escapeMarkup |> Console.yellow) |> Option.defaultValue "" |> render

        let content (doc: Document option) =
            match doc with
            | None -> ""
            | Some doc -> 
                seq {
                    (if doc.description.Length > 0 then doc.description else "") |> Strings.escapeMarkup |> Console.lightcyan
                    (if doc.content.Length > 0 then doc.content else "") |> Strings.escapeMarkup
                } |> Seq.filter (fun x -> x.Length > 0) |> Strings.join System.Environment.NewLine
            |> render
        
        layout.["title"].Update(title document) |> ignore
        layout.["uri"].Update(uri document) |> ignore
        layout.["pub"].Update(pub document) |> ignore
        layout.["content"].Update(content document) |> ignore                    
        layout


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

type CycleDocumentsCommand() =    
    inherit AsyncCommand<DocumentsCommandSettings>()
    
    let mainLayout = DocumentsConsole.screenLayout ()
                
    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
                        
            do! AnsiConsole.Live(mainLayout)
                    .AutoClear(true)
                    .StartAsync(fun ctx -> 
                        task {
                            let mutable quit = false
                            while not quit do
                                // TODO: spinner
                                let! r = DiscorssApi.nextDocument settings.ApiHost
                                
                                r |> DocumentsConsole.updateDocumentsLayout mainLayout |> ignore
                                                                
                                ctx.UpdateTarget(mainLayout)

                                let mutable nextDoc = false
                                while not nextDoc do
                                    match System.Console.ReadKey(true).Key with
                                    | System.ConsoleKey.Q -> 
                                        quit <- true
                                        nextDoc <- true
                                    | System.ConsoleKey.O -> 
                                        r |> Option.map (_.uri >> Process.openUri) |> ignore                                                                        
                                    | _ -> nextDoc <- true
                                           

                            
                        })

            return ReturnCodes.ok
        }

    interface ICommandLimiter<CommandSettings>
