namespace Discorss.Server

open Discorss
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

type Startup() =

    let serviceCollection = Discorss.Feeds.Service.WebApp.services // TODO: append more

    member __.ConfigureServices(services: IServiceCollection) =

        services
        |> ApiStartup.addApi
        |> serviceCollection
        |> ignore

    member __.Configure (app: IApplicationBuilder) (env: IHostEnvironment) (loggerFactory: ILoggerFactory) =
        let apis = Discorss.Feeds.Service.WebApp.webApp app.ApplicationServices // TODO: merge from all
        
        app.UseGiraffeErrorHandler(Api.errorHandler)
           .UseHttpLogging()
           .UseGiraffe(apis)


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
