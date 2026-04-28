namespace Discorss.Ingestion.Service

open System
open Discorss
open Giraffe
open Microsoft.Extensions.DependencyInjection

module WebApp =
    let services (services: IServiceCollection) =
        services.AddSingleton<Ingestion.IngestionActor>()

    let webApp (sp: IServiceProvider) =

        subRouteCi
            "/api/v1/ingestion"
            (Api.logClient
             >=> Api.isAuthorised sp
             >=> choose
                     [ GET
                       >=> choose
                               [ Discorss.Api.heartbeatRoute
                                 route "/stats" >=> WebAppHandlers.getActorStats sp ]
                       POST >=> choose [ route "/ingest" >=> WebAppHandlers.testIngestion sp ] ])
