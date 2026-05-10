namespace Discorss.Server

open System
open System.Threading.Tasks
open Discorss
open Discorss.Ingestion
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers =

    let getStats (sp: IServiceProvider) =

        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let actorTasks =
                    [| sp.GetRequiredService<Ingestion.QueueMonitorActor>() :> IActor
                       sp.GetRequiredService<Ingestion.IngestionActor>() :> IActor
                       sp.GetRequiredService<Ingestion.FeedIngestionActor>() :> IActor
                       sp.GetRequiredService<Ingestion.DocumentIngestionActor>() :> IActor |]
                    |> Array.map _.GetStats()

                let! actorStats = Task.WhenAll actorTasks

                let results = actorStats |> Array.map Models.toStats |> Array.sortBy _.name

                return! Successful.OK results next ctx
            }
