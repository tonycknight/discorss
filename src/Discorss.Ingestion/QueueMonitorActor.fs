namespace Discorss.Ingestion

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

    let pollQueue queueName =
        task {
            try
                log.LogTrace $"Polling queue {queueName}..."
                do! Actor.pollActorMessageQueue broker queueName (Actor.post ingestionActor)
            with ex ->
                log.LogError(ex, "Error polling queue")

            do! Task.delay config.Value.queuePollFrequency

            queueName |> ActorMessage.PollQueue |> Actor.post self
        }

    let start () =
        task {
            for name in ingestionActor.QueueNames do
                do! pollQueue name
        }

    let processMessage (inbox: MailboxProcessor<ActorMessage>) (state: ActorState<unit>) =
        task {
            let! msg = inbox.Receive()

            let! state =
                match msg with
                | ActorMessage.PollQueue queueName ->
                    task {
                        do! pollQueue queueName
                        return state
                    }
                | ActorMessage.Start ->
                    task {
                        do! start ()
                        return state
                    }
                | ActorMessage.Stop rc ->
                    rc.Reply()
                    { state with stopped = true } |> Task.ofResult
                | _ -> state |> Task.ofResult

            return state
        }

    let rec loop inbox state =
        async {
            let! state = processMessage inbox state |> Async.AwaitTask

            return!
                match state.stopped with
                | true -> async { }
                | _ -> loop inbox state
        }

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox -> loop inbox { stopped = false; state = () })

    interface IStatsSource with
        member this.GetStatsAsync() =
            actor |> Actor.getStats (self.GetType().Name) |> Task.ofResult

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg
        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)

        member this.Stop() =
            actor.PostAndReply(fun rc -> ActorMessage.Stop rc)
