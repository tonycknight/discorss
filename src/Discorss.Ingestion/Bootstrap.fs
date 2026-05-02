namespace Discorss.Ingestion

open System
open Discorss
open Microsoft.Extensions.DependencyInjection

module Bootstrap =
    let services (services: IServiceCollection) =
        services
            .AddSingleton<Ingestion.IngestionActor>()
            .AddSingleton<Ingestion.FeedIngestionActor>()

    let start (sp: IServiceProvider) = 
        let actor = sp.GetRequiredService<Ingestion.IngestionActor>()
        ActorMessage.Start |> Actor.post actor