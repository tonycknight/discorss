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
                    |> Array.map _.GetStats() // TODO: move actors to use IStatsSource

                let repoTasks =
                    [| sp.GetRequiredService<Documents.IDocumentRepository>() :?> IStatsSource |]
                    |> Array.filter (fun x -> Object.ReferenceEquals(x, null) |> not)
                    |> Array.map _.GetStatsAsync()

                let! actorStats = Task.WhenAll actorTasks
                let! repoStats = Task.WhenAll repoTasks

                let stats = actorStats |> Array.append repoStats

                let results = stats |> Array.map Models.toStats |> Array.sortBy _.name

                return! Successful.OK results next ctx
            }
