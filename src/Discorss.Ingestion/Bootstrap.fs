namespace Discorss.Ingestion

open Discorss
open Microsoft.Extensions.DependencyInjection

module Bootstrap =
    let services (services: IServiceCollection) =
        services
            .AddSingleton<Ingestion.IngestionActor>()
            .AddSingleton<Ingestion.FeedIngestionActor>()
            .AddSingleton<Ingestion.QueueMonitorActor>()
