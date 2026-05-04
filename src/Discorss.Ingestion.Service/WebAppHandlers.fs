namespace Discorss.Ingestion.Service

open System
open System.Threading.Tasks
open Discorss
open Discorss.Ingestion
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers =

    let ingestionActor (sp: IServiceProvider) = sp.GetRequiredService<Ingestion.IngestionActor>() :> IActor
    let feedActor (sp: IServiceProvider) = sp.GetRequiredService<Ingestion.FeedIngestionActor>() :> IActor
    let docActor (sp: IServiceProvider) = sp.GetRequiredService<Ingestion.DocumentIngestionActor>() :> IActor
    let queueActor (sp: IServiceProvider) = sp.GetRequiredService<Ingestion.QueueMonitorActor>() :> IActor

    let getActorStats (sp: IServiceProvider) =

        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let results = 
                    [| 
                        (ingestionActor sp).GetStats()
                        (docActor sp).GetStats()
                        (queueActor sp).GetStats()
                        (feedActor sp).GetStats()
                    |]
                                    
                return! Successful.OK results next ctx
            }
