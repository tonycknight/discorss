namespace Discorss.MessageHub.Service

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
                                                // TODO: hub stats
                                                route "/queues/" >=> WebAppHandlers.getQueueNames sp
                                                routeCif "/queues/%s/next/" (fun name -> WebAppHandlers.getNextMessage sp name) >=> (responseCaching CacheDirective.NoCache None None)
                                            ]
                            PUT >=> choose [                                                    
                                                routeCif "/queues/%s/" (fun name -> WebAppHandlers.pushMessage sp name) 
                                            ]
                            ]            
            )

