namespace Discorss.Server

open System
open Giraffe

module WebApp =

    let webApp path (sp: IServiceProvider) =

        subRouteCi path 
            (choose 
                [ GET >=> 
                    choose 
                        [ route "stats/" >=> WebAppHandlers.getActorStats sp 
                          route "heartbeat/" >=> noResponseCaching >=> json [ "OK" ]
                        ] ])
