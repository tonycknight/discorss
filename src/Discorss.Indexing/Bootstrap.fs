namespace Discorss.Indexing

open Microsoft.Extensions.DependencyInjection

module Bootstrap =
    let services (services: IServiceCollection) =
        services
            .AddSingleton<Discorss.Indexing.IDocumentStatsWriter, Discorss.Indexing.StubDocumentStatsWriter>()