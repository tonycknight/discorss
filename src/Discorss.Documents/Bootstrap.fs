namespace Discorss.Documents

open Microsoft.Extensions.DependencyInjection

module Bootstrap =

    let services (services: IServiceCollection) =
        services.AddSingleton<IDocumentRepository, StubDocumentRepository>()
