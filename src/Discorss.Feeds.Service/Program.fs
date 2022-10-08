namespace Discorss.Feeds.Service

open System
open Discorss
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
    

type Startup() =
    
    member __.ConfigureServices (services : IServiceCollection) =

        let addDeps(services : IServiceCollection) =
            services.AddSingleton<Discorss.Feeds.IFeedRepository, Discorss.Feeds.StubFeedRepository>()
                    .AddSingleton<Discorss.Feeds.IFeedProvider, Discorss.Feeds.FeedProvider>()                               

        services    |> ApiStartup.addApiLogging
                    |> ApiStartup.addApiConfig
                    |> ApiStartup.addApiHttp
                    |> ApiStartup.addApi
                    |> addDeps
                    |> ignore
        
    member __.Configure (app : IApplicationBuilder)
                        (env : IHostEnvironment)
                        (loggerFactory : ILoggerFactory) =       
        app.UseHttpLogging()
           .UseGiraffe (WebApp.webApp app.ApplicationServices)
        
    
module Program=
    
    [<EntryPoint>]
    let main _ =
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(fun webHostBuilder -> webHostBuilder.UseStartup<Startup>() |> ignore)
            .Build()
            .Run()
        0