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

type CycleDocumentsCommand() =    
    inherit AsyncCommand<DocumentsCommandSettings>()

    let titlePanel = Layout("title").Size(1)
    let uriPanel = Layout("uri").Size(1)
    let pubPanel = Layout("pub").Size(1)
    let contentPanel = Layout("content").MinimumSize(1)    
    let layoutRows = [| titlePanel; uriPanel; pubPanel; contentPanel |]    
    let mainLayout = Layout().SplitRows(layoutRows)
                
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

                                match r with
                                | None -> 
                                    titlePanel.Update("No document found." |> Console.red |> render) |> ignore
                                    uriPanel.Update("" |> render) |> ignore
                                    pubPanel.Update("" |> render) |> ignore
                                    contentPanel.Update("" |> render) |> ignore                                    
                                | Some doc ->
                                    titlePanel.Update(doc.title |> Strings.escapeMarkup |> Console.cyan |> render) |> ignore
                                    uriPanel.Update(doc.uri |> Strings.escapeMarkup |> Console.yellow |> render) |> ignore
                                    
                                    let pub = 
                                        seq { 
                                            doc.publication.ToString()
                                            doc.author |> Strings.escapeMarkup
                                        } |> Seq.filter (fun x -> x <> "") |> Strings.join " - "
                                        |> Console.grey |> Console.italic |> render
                                    pubPanel.Update(pub) |> ignore                                    
                                                                        
                                    let content = 
                                        seq {
                                            (if doc.description.Length > 0 then doc.description else "") |> Strings.escapeMarkup |> Console.lightcyan
                                            (if doc.content.Length > 0 then doc.content else "") |> Strings.escapeMarkup
                                        } |> Seq.filter (fun x -> x.Length > 0) |> Strings.join System.Environment.NewLine

                                    contentPanel.Update(content |> render) |> ignore

                                // TODO: key instructions at the bottom?
                                ctx.UpdateTarget(mainLayout)

                                let key = System.Console.ReadKey(true)
                                quit <- (key.Key = System.ConsoleKey.Q)
                            
                        })

            return ReturnCodes.ok
        }

    interface ICommandLimiter<CommandSettings>
