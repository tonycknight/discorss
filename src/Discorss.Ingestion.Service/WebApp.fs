namespace Discorss.Ingestion.Service

open System
open Giraffe

module WebApp =

    let webApp path (sp: IServiceProvider) =

        subRouteCi
            path
            (choose
                [ GET >=> choose [ route "/stats" >=> WebAppHandlers.getActorStats sp ]
                  POST >=> choose [ route "/ingest" >=> WebAppHandlers.testIngestion sp ] ])
