namespace Discorss.Feeds.Service

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
    

type Startup() =
    
    member __.ConfigureServices (services : IServiceCollection) =
        services.AddLogging()
                .AddHttpLogging(fun lo -> lo.LoggingFields <- Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All)
                .AddHttpClient()
                .AddSingleton<Discorss.IExternalHttpClient, Discorss.ExternalHttpClient>()
                .AddSingleton<Discorss.IExternalHttpClientFactory, Discorss.ExternalHttpClientFactory>()
                .AddSingleton<Discorss.Feeds.IFeedRepository, Discorss.Feeds.StubFeedRepository>()
                .AddSingleton<Discorss.Security.ISecretProvider, Discorss.Security.StubSecretProvider>()
                .AddGiraffe() 
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