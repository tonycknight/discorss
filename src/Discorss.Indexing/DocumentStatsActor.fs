namespace Discorss.Indexing

open Discorss.Documents

type private DocStatsCache = Map<string, DocumentStatistics>

type DocumentStatsActor(parent: IActor) =

    let key (s: string) = s.ToLowerInvariant()

    let add (cache: DocStatsCache) (stats: DocumentStatistics) =
        let key = key stats.uri
        cache |> Map.add key stats

    let actor =
        MailboxProcessor<StatsActorMessage>.Start(fun inbox ->
            let rec loop (state: DocStatsCache) =
                async {
                    let! msg = inbox.Receive()

                    let state =
                        match msg with
                        | StatsActorMessage.Stats docStats -> docStats |> add state
                        | m ->
                            parent.Post m
                            state

                    return! loop state
                }

            Map.empty<string, DocumentStatistics> |> loop)

    interface IActor with
        member this.Post(msg: StatsActorMessage) = actor.Post msg
        member this.ReplyAsync(msg: StatsActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
