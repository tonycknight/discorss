namespace Discorss.MessageHub.Service

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
        services    |> ApiStartup.addApi
                    |> (fun s -> s.AddSingleton<IQueueProvider, QueueProvider>())
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
            .ConfigureWebHostDefaults(fun whb -> whb.UseStartup<Startup>()
                                                    .UseUrls($"http://+:{Configuration.ApiPorts.hubServicePort}") |> ignore)
            .Build()
            .Run()
        0