namespace Discorss.Ingestion.Service

open System
open Discorss
open Giraffe
open Microsoft.Extensions.DependencyInjection

module WebApp =
    let services (services: IServiceCollection) =
        services.AddSingleton<Ingestion.IngestionActor>()
        
    let webApp path (sp: IServiceProvider) =

        subRouteCi
            path
            (choose
                [ GET >=> choose [ route "/stats" >=> WebAppHandlers.getActorStats sp ]
                  POST >=> choose [ route "/ingest" >=> WebAppHandlers.testIngestion sp ] ])
