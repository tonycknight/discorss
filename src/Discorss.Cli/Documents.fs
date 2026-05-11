namespace Discorss

open Discorss.ApiModels
open Spectre.Console
open Spectre.Console.Cli

type DocumentsCommandSettings() =
    inherit BaseCommandSettings()

module DocumentsConsole =
    let private render value = value |> Console.markup |> Console.renderable

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

    let documentsLayout () =
        let titlePanel = Layout("title").Size(1)
        let uriPanel = Layout("uri").Size(1)
        let pubPanel = Layout("pub").Size(1)
        let contentPanel = Layout("content").MinimumSize(1)    
        let layoutRows = [| titlePanel; uriPanel; pubPanel; contentPanel |]    
        Layout().SplitRows(layoutRows)
        
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
    
    let mainLayout = DocumentsConsole.documentsLayout ()
                
    override this.ExecuteAsync(context, settings, cancellationToken) =
        task {
            let render value = value |> Console.markup |> Console.renderable
            
            do! AnsiConsole.Live(mainLayout)
                    .AutoClear(true)
                    .StartAsync(fun ctx -> 
                        task {
                            let mutable quit = false
                            while not quit do
                                // TODO: spinner
                                let! r = DiscorssApi.nextDocument settings.ApiHost
                                
                                r |> DocumentsConsole.updateDocumentsLayout mainLayout |> ignore

                                // TODO: key instructions at the bottom?
                                ctx.UpdateTarget(mainLayout)

                                let key = System.Console.ReadKey(true)
                                quit <- (key.Key = System.ConsoleKey.Q)
                            
                        })

            return ReturnCodes.ok
        }

    interface ICommandLimiter<CommandSettings>
