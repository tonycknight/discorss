namespace Discorss.Feeds.Service

open System
open Discorss
open Giraffe
   
module WebApp=
    let webApp(sp: IServiceProvider) =    
               
        subRouteCi "/api/v1" 
                (   
                    Api.logClient >=> Api.isAuthorised sp >=>
                        choose [
                                GET >=> choose  [    
                                                    Discorss.Api.heartbeatRoute
                                                    routeCif "/feeds/%s/" (fun url -> publicResponseCaching 5 None >=> WebAppHandlers.getFeed sp url)
                                                    route "/feeds/" >=> (noResponseCaching >=> WebAppHandlers.getFeeds sp)
                                                ]                                
                                ]
                )



