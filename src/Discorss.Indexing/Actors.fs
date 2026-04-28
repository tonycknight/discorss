namespace Discorss.Indexing

type StatsActorMessage =
    | Stop
    | Start
    | Stats of DocumentStatistics

type IActor =
    abstract member Post: StatsActorMessage -> unit
    abstract member ReplyAsync: StatsActorMessage -> Async<StatsActorMessage>
