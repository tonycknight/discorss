namespace Discorss.MessageHub.Service

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
                                                // TODO: hub stats
                                                route "/queue/" >=> WebAppHandlers.getQueueNames sp
                                                routeCif "/queue/%s/head/" (fun name -> noResponseCaching >=> WebAppHandlers.getNextMessage sp name)
                                            ]
                            POST >=> choose [                                                    
                                                routeCif "/queue/%s/" (fun name -> WebAppHandlers.pushMessage sp name) 
                                            ]
                            ]            
            )

