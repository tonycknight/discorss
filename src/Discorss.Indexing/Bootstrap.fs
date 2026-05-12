namespace Discorss.Indexing

open Microsoft.Extensions.DependencyInjection

module Bootstrap =
    let services (services: IServiceCollection) =
        services
            .AddSingleton<Discorss.Indexing.ILexicon, Discorss.Indexing.Lexicon>()
            .AddSingleton<Discorss.Indexing.IDocumentAnalyser, Discorss.Indexing.DocumentAnalyser>()
            .AddSingleton<Discorss.Indexing.IDocumentStatsWriter, Discorss.Indexing.StubDocumentStatsWriter>()