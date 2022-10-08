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

        let addDeps(services : IServiceCollection) =
            services.AddSingleton<Discorss.Indexing.ILexicon, Discorss.Indexing.Lexicon>()
                    .AddSingleton<Discorss.Indexing.IDocumentAnalyser, Discorss.Indexing.DocumentAnalyser>()                

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
            .ConfigureWebHostDefaults(
                fun webHostBuilder -> webHostBuilder.UseStartup<Startup>()
                                        |> ignore)
            .Build()
            .Run()
        0