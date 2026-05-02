namespace Discorss.Server

open Discorss
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

type Startup() =

    let serviceCollection =
        Discorss.Feeds.Service.WebApp.services
        >> Discorss.Indexing.Service.WebApp.services
        >> Discorss.Ingestion.Bootstrap.services

    let routes (app: IApplicationBuilder) =
        subRouteCi
            "/api/v1"
            (Api.logClient
             >=> choose
                     [ GET >=> Discorss.Api.heartbeatRoute
                       Discorss.Feeds.Service.WebApp.webApp "/feeds" app.ApplicationServices
                       Discorss.Indexing.Service.WebApp.webApp "/indexing" app.ApplicationServices
                       Discorss.Ingestion.Service.WebApp.webApp "/ingestion" app.ApplicationServices ])

    member __.ConfigureServices(services: IServiceCollection) =

        services |> ApiStartup.addApi |> serviceCollection |> ignore

    member __.Configure (app: IApplicationBuilder) (env: IHostEnvironment) (loggerFactory: ILoggerFactory) =

        app.UseGiraffeErrorHandler(Api.errorHandler).UseHttpLogging().UseGiraffe(routes app)


module Program =

    [<EntryPoint>]
    let main _ =
        Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(fun whb ->
                whb
                    .UseStartup<Startup>()
                    .UseUrls($"http://*:{ApiPorts.servicePort}")
                    .ConfigureAppConfiguration(ApiStartup.configureAppConfig)
                |> ignore)
            .Build()
            .Run()

        0
