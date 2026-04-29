namespace Discorss.Feeds.Service

open System
open Discorss
open Giraffe
open Microsoft.Extensions.DependencyInjection

module WebApp =
    let services (services: IServiceCollection) =
        services
            .AddSingleton<Discorss.Feeds.IFeedRepository, Discorss.Feeds.StubFeedRepository>()
            .AddSingleton<Discorss.Feeds.IFeedProvider, Discorss.Feeds.FeedProvider>()

    let webApp path (sp: IServiceProvider) =

        subRouteCi
            path
            (Api.logClient
             >=> Api.isAuthorised sp
             >=> choose
                     [ GET
                       >=> choose
                               [ routeCif "/%s/" (fun url ->
                                     publicResponseCaching 5 None >=> WebAppHandlers.getFeed sp url)
                                 route "/" >=> (noResponseCaching >=> WebAppHandlers.getFeeds sp) ] ])
