namespace Discorss.Indexing

open Discorss.Documents

type private WordStatsCache = Map<string, WordStatistics>

type WordStatsActor(parent: IActor, repo: IWordStatisticsRepository) =

    let actor =
        MailboxProcessor<StatsActorMessage>.Start(fun inbox ->
            let rec loop (state: WordStatsCache) =
                async {
                    let! msg = inbox.Receive()

                    let state =
                        match msg with
                        | StatsActorMessage.Stats stats -> state // TODO: stats |> add state
                        | m ->
                            parent.Post m
                            state

                    return! loop state
                }

            Map.empty<string, WordStatistics> |> loop)

    interface IActor with
        member this.Post(msg: StatsActorMessage) = actor.Post msg
        member this.ReplyAsync(msg: StatsActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
