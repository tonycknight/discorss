namespace Discorss.Documents

open Discorss
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

module Bootstrap =

    let services (services: IServiceCollection) =
        use sp = services.BuildServiceProvider()
        let config = sp.GetRequiredService<IOptions<AppConfiguration>>()

        let services =
            if config.Value.mongoConnection |> String.isEmptyWhitespace then
                services
                    .AddSingleton<IDocumentRepository, StubDocumentRepository>()
                    .AddSingleton<IDocumentStatisticsRepository, StubDocumentStatisticsRepository>()
                    .AddSingleton<IDocumentLikeRepository, StubDocumentLikeRepository>()
            else
                services
                    .AddSingleton<IDocumentRepository, MongoDocumentRepository>()
                    .AddSingleton<IDocumentStatisticsRepository, MongoDocumentStatisticsRepository>()
                    .AddSingleton<IDocumentLikeRepository, MongoDocumentLikeRepository>()

        services
            .AddSingleton<IDocumentNotificationWriter, DocumentNotificationWriter>()
            .AddSingleton<IDocumentNotificationReader, DocumentNotificationReader>()
            .AddSingleton<ILexicon, Lexicon>()
            .AddSingleton<IDocumentAnalyser, DocumentAnalyser>()
