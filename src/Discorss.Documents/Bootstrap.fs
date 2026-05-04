namespace Discorss.Documents

open Discorss
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

module Bootstrap =

    let services (services: IServiceCollection) =
        use sp = services.BuildServiceProvider()
        let config = sp.GetRequiredService<IOptions<AppConfiguration>>()
        if config.Value.mongoConnection |> Strings.isEmptyWhitespace then
            services.AddSingleton<IDocumentRepository, StubDocumentRepository>()
        else
            services.AddSingleton<IDocumentRepository, MongoDocumentRepository>()
