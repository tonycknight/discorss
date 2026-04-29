namespace Discorss.Indexing.Service

open System
open Discorss
open Giraffe
open Microsoft.Extensions.DependencyInjection

module WebApp =
    let services (services: IServiceCollection) =
        services
            .AddSingleton<Discorss.Indexing.ILexicon, Discorss.Indexing.Lexicon>()
            .AddSingleton<Discorss.Indexing.IDocumentAnalyser, Discorss.Indexing.DocumentAnalyser>()
            .AddSingleton<Discorss.Indexing.IDocumentStatsWriter, Discorss.Indexing.StubDocumentStatsWriter>()

    let webApp path (sp: IServiceProvider) =

        subRouteCi
            path
            (Api.logClient
             >=> Api.isAuthorised sp
             >=> choose
                     [ POST
                       >=> choose
                               [ route "/stats" >=> WebAppHandlers.getDocumentStatistics sp
                                 route "/words" >=> WebAppHandlers.getDocumentWords sp ]
                       PUT >=> choose [ route "/index" >=> WebAppHandlers.setDocument sp ] ])
