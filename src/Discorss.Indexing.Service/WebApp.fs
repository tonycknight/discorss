namespace Discorss.Indexing.Service

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
                                            ]
                            POST >=> choose [
                                                route "/stats" >=> WebAppHandlers.getDocumentStatistics sp
                                                route "/words" >=> WebAppHandlers.getDocumentWords sp                                                
                                            ]   
                            PUT >=> choose  [
                                                route "/index" >=> WebAppHandlers.setDocument sp
                                            ]
                            ]            
            )

