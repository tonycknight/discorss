namespace Discorss.Ingestion

type IActor =
    abstract member Post: ActorMessage -> unit
    abstract member ReplyAsync: ActorMessage -> Async<ActorMessage>
    abstract member GetStats: unit -> Async<ActorStats>


module Actor =
    let getStats name (mailbox: MailboxProcessor<ActorMessage>) =
        { ActorStats.name = name
          queueCount = mailbox.CurrentQueueLength
          childStats = [] }
