namespace Discorss.Feeds.Service

open System
open Discorss
open Giraffe

module WebApp =

    let webApp path (sp: IServiceProvider) =

        subRouteCi
            path
            (Api.logClient
             >=> choose
                     [ GET
                       >=> choose
                               [ routeCif "/%s/" (fun url ->
                                     publicResponseCaching 5 None >=> WebAppHandlers.getFeed sp url)
                                 route "/" >=> (noResponseCaching >=> WebAppHandlers.getFeeds sp) ]
                       PUT >=> choose [ route "/" >=> WebAppHandlers.setFeed sp ] ])
