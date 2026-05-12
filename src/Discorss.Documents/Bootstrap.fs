namespace Discorss.Documents

open Discorss
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

module Bootstrap =

    let services (services: IServiceCollection) =
        use sp = services.BuildServiceProvider()
        let config = sp.GetRequiredService<IOptions<AppConfiguration>>()

        let services =
            if config.Value.mongoConnection |> Strings.isEmptyWhitespace then
                services.AddSingleton<IDocumentRepository, StubDocumentRepository>()
            else
                services.AddSingleton<IDocumentRepository, MongoDocumentRepository>()

        services
            .AddSingleton<IDocumentNotificationWriter, DocumentNotificationWriter>()
            .AddSingleton<IDocumentNotificationReader, DocumentNotificationReader>()
            .AddSingleton<ILexicon, Lexicon>()
            .AddSingleton<IDocumentAnalyser, DocumentAnalyser>()
            .AddSingleton<IWordStatisticsRepository, MemoryWordStatisticsRepository>()
            .AddSingleton<IDocumentStatisticsRepository, MemoryDocumentStatisticsRepository>()
            
            
