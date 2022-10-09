namespace Discorss.Indexing.Service

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
                    |> (fun s -> s.AddSingleton<Discorss.Indexing.ILexicon, Discorss.Indexing.Lexicon>()
                                  .AddSingleton<Discorss.Indexing.IDocumentAnalyser, Discorss.Indexing.DocumentAnalyser>()
                                  .AddSingleton<Discorss.Indexing.IDocumentStatsWriter, Discorss.Indexing.StubDocumentStatsWriter>())
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
                                                    .UseUrls($"http://+:{ApiPorts.indexServicePort}") 
                                                    .ConfigureAppConfiguration(ApiStartup.configureAppConfig)
                                                    |> ignore)
            .Build()
            .Run()
        0