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
                let actors =
                    [| sp.GetRequiredService<Ingestion.QueueMonitorActor>() :> IActor
                       sp.GetRequiredService<Ingestion.IngestionActor>() :> IActor
                       sp.GetRequiredService<Ingestion.FeedIngestionActor>() :> IActor
                       sp.GetRequiredService<Ingestion.DocumentIngestionActor>() :> IActor |]

                let results = actors |> Array.map _.GetStats() |> Array.sortBy (fun x -> x.name)

                return! Successful.OK results next ctx
            }
