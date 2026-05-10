namespace Discorss.Server

open System
open System.Threading.Tasks
open Discorss
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers =

    let getStats (sp: IServiceProvider) =

        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let statsTasks =
                    [| sp.GetRequiredService<Documents.IDocumentRepository>() :?> IStatsSource
                       sp.GetRequiredService<Feeds.IFeedRepository>() :?> IStatsSource
                       sp.GetRequiredService<Documents.IDocumentNotificationReader>() :?> IStatsSource
                       sp.GetRequiredService<Ingestion.QueueMonitorActor>() :> IStatsSource
                       sp.GetRequiredService<Ingestion.IngestionActor>() :> IStatsSource
                       sp.GetRequiredService<Ingestion.FeedIngestionActor>() :> IStatsSource
                       sp.GetRequiredService<Ingestion.DocumentIngestionActor>() :> IStatsSource |]
                    |> Array.filter (fun x -> Object.ReferenceEquals(x, null) |> not)
                    |> Array.map _.GetStatsAsync()

                let! stats = Task.WhenAll statsTasks

                let results = stats |> Array.map Models.toStats |> Array.sortBy _.name

                return! Successful.OK results next ctx
            }
