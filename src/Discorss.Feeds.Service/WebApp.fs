namespace Discorss.Feeds.Service

open System
open Discorss
open Giraffe
   
module WebApp=
    let webApp(sp: IServiceProvider) =    
               
        subRouteCi "/api/v1" 
                (   
                    Api.isAuthorised sp >=>
                        choose [
                                GET >=> choose  [    
                                                    Discorss.Api.heartbeatRoute
                                                    route "/feeds" >=> (WebAppHandlers.getFeeds sp)
                                                ]
                                POST >=> choose [
                                                    route "/preview" >=> (WebAppHandlers.previewFeed sp)
                                                ]   
                                ]
                )



