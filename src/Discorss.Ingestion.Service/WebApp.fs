namespace Discorss.Ingestion.Service

open System
open Discorss
open Giraffe

module WebApp=
    let webApp (sp: IServiceProvider) = 

        subRouteCi "/api/v1" 
            (   
                Api.isAuthorised sp >=>
                    choose [
                            GET >=> choose  [    
                                                Discorss.Api.heartbeatRoute
                                            ]
                            POST >=> choose [
                                                route "/ingest" >=> WebAppHandlers.testIngestion sp
                                            ]
                            ]            
            )

