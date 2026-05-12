namespace Discorss.Server

open System
open Discorss
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

type Startup() =

    let serviceCollection =
        Discorss.Feeds.Bootstrap.services
        >> Discorss.Ingestion.Bootstrap.services
        >> Discorss.Documents.Bootstrap.services

    let routes (app: IApplicationBuilder) =
        subRouteCi
            "/api/v1"
            (Api.logClient
             >=> choose
                     [ Discorss.Server.WebApp.webApp "/" app.ApplicationServices
                       Discorss.Feeds.Service.WebApp.webApp "/feeds" app.ApplicationServices
                       Discorss.Documents.Service.WebApi.webApp "/documents" app.ApplicationServices ])

    member __.ConfigureServices(services: IServiceCollection) =

        services |> ApiStartup.addApi |> serviceCollection |> ignore

    member __.Configure (app: IApplicationBuilder) (env: IHostEnvironment) (loggerFactory: ILoggerFactory) =

        app.UseGiraffeErrorHandler(Api.errorHandler).UseHttpLogging().UseGiraffe(routes app)


module Program =

    open Discorss.Ingestion

    let startup (sp: IServiceProvider) =

        let actors =
            [| sp.GetRequiredService<IngestionActor>() :> IActor
               sp.GetRequiredService<QueueMonitorActor>() :> IActor |]

        actors |> Array.iter (fun a -> ActorMessage.Start |> a.Post)



    [<EntryPoint>]
    let main args =
        let host =
            Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(fun whb ->
                    whb
                        .UseStartup<Startup>()
                        .UseUrls($"http://*:{Api.servicePort}")
                        .ConfigureAppConfiguration(ApiStartup.configureAppConfig args)
                    |> ignore)
                .Build()

        host.Services |> startup

        host.Run()

        0
