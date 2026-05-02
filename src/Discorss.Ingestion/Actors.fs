namespace Discorss.Ingestion

type IActor =
    abstract member Post: ActorMessage -> unit
    abstract member ReplyAsync: ActorMessage -> Async<ActorMessage>
    abstract member GetStats: unit -> Async<ActorStats>    

module Actor =
    open System

    let post<'a> actor message =
        (actor :> IActor).Post message

    let getStats name (mailbox: MailboxProcessor<ActorMessage>) =
        { ActorStats.name = name
          queueCount = mailbox.CurrentQueueLength
          childStats = [] }

    let createTimer (duration: TimeSpan) (func: obj -> unit) =
        let f source args = func args

        let handler = new System.Timers.ElapsedEventHandler(f)
        let result = new System.Timers.Timer(duration)
        result.Elapsed.AddHandler(handler)
        result.Enabled <- false
        result.AutoReset <- true
        result

    let pullActorMessage (broker: Microbroker.Client.IMicrobrokerProxy) queueName =
        task {
            let! msg = broker.GetNextAsync queueName

            return
                match msg with
                | Some m -> m |> Discorss.Queues.Messages.fromQueueMessage<ActorMessage> 
                | None -> None
        }