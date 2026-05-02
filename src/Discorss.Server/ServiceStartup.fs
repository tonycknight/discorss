namespace Discorss.Server

open System
open System.Threading
open System.Threading.Tasks
open Discorss
open Discorss.Ingestion
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

type ServiceStartup(logFactory: ILoggerFactory, config: AppConfiguration, ingestionActor: IngestionActor) =
    
    inherit BackgroundService()

    let log = logFactory.CreateLogger<ServiceStartup>()

    override this.ExecuteAsync(cancellationToken: CancellationToken) : Task =
        task {
            
            log.LogTrace $"Starting ingestion..."

            ActorMessage.Start |> Actor.post ingestionActor
        }