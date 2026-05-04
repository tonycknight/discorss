namespace Discorss.Feeds

open Discorss
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

module Bootstrap =
    let services (services: IServiceCollection) =
        use sp = services.BuildServiceProvider()
        let config = sp.GetRequiredService<IOptions<AppConfiguration>>()

        let services =
            if config.Value.mongoConnection |> Strings.isEmptyWhitespace then
                services.AddSingleton<Discorss.Feeds.IFeedRepository, Discorss.Feeds.StubFeedRepository>()
            else
                services.AddSingleton<Discorss.Feeds.IFeedRepository, Discorss.Feeds.MongoFeedRepository>()

        services.AddSingleton<Discorss.Feeds.IFeedProvider, Discorss.Feeds.FeedProvider>()
