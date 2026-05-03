namespace Discorss.Feeds

open Microsoft.Extensions.DependencyInjection

module Bootstrap =
    let services (services: IServiceCollection) =
        services
            .AddSingleton<Discorss.Feeds.IFeedRepository, Discorss.Feeds.StubFeedRepository>()
            .AddSingleton<Discorss.Feeds.IFeedProvider, Discorss.Feeds.FeedProvider>()
