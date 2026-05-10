namespace Discorss.Server

open System
open Giraffe

module WebApp =

    let webApp path (sp: IServiceProvider) =

        subRouteCi
            path
            (choose
                [ GET
                  >=> choose
                          [ route "stats/" >=> publicResponseCaching 5 None >=> WebAppHandlers.getStats sp
                            route "heartbeat/" >=> noResponseCaching >=> json [ "OK" ] ] ])
