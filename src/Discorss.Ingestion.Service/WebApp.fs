namespace Discorss.Ingestion.Service

open System
open Discorss
open Giraffe

module WebApp=
    let webApp (sp: IServiceProvider) = 

        subRouteCi "/api/v1" 
            (   
                Api.logClient >=> Api.isAuthorised sp >=>
                    choose [
                            GET >=> choose  [    
                                                Discorss.Api.heartbeatRoute
                                                route "/stats" >=> WebAppHandlers.getActorStats sp
                                            ]
                            POST >=> choose [
                                                route "/ingest" >=> WebAppHandlers.testIngestion sp
                                            ]
                            ]            
            )

