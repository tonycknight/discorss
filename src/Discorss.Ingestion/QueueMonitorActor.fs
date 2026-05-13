namespace Discorss.Ingestion

open System
open System.Diagnostics.CodeAnalysis
open Discorss
open Microbroker.Client
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

[<ExcludeFromCodeCoverage>]
type QueueMonitorActor
    (
        logFactory: ILoggerFactory,
        config: IOptions<AppConfiguration>,
        broker: IMicrobrokerProxy,
        ingestionActor: IngestionActor
    ) as self =

    let log = logFactory.CreateLogger<QueueMonitorActor>()

    let timers =
        ingestionActor.QueueNames
        |> List.map (fun queueName -> fun args -> queueName |> ActorMessage.PollQueue |> Actor.post self)
        |> List.map (Actor.createTimer config.Value.queuePollFrequency)

    let processMessage (inbox: MailboxProcessor<ActorMessage>) =
        task {
            let! msg = inbox.Receive()

            match msg with
            | ActorMessage.PollQueue queueName ->
                log.LogTrace $"Polling queue {queueName}..."
                do! Actor.pollActorMessageQueue broker queueName (Actor.post ingestionActor)
            | ActorMessage.Start -> do timers |> List.iter (fun t -> t.Enabled <- true)
            | ActorMessage.Stop -> do timers |> List.iter (fun t -> t.Enabled <- false)
            | _ -> ignore msg
        }

    let rec loop inbox =
        async {
            do! processMessage inbox |> Async.AwaitTask

            return! loop inbox
        }

    let actor = MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox)

    interface IStatsSource with
        member this.GetStatsAsync() =
            actor |> Actor.getStats (self.GetType().Name) |> Task.ofResult

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
