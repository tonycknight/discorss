namespace Discorss.Ingestion.Service

open System
open System.Threading.Tasks
open Discorss
open Discorss.Ingestion
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers =

    let getActorStats (sp: IServiceProvider) =

        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let tasks =
                    [| sp.GetRequiredService<Ingestion.QueueMonitorActor>() :> IActor
                       sp.GetRequiredService<Ingestion.IngestionActor>() :> IActor
                       sp.GetRequiredService<Ingestion.FeedIngestionActor>() :> IActor
                       sp.GetRequiredService<Ingestion.DocumentIngestionActor>() :> IActor |]
                    |> Array.map _.GetStats()

                let! results = Task.WhenAll tasks

                return! Successful.OK (results |> Array.sortBy _.name) next ctx
            }
