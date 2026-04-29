namespace Discorss.Ingestion

open System.Diagnostics.CodeAnalysis
open Discorss

[<ExcludeFromCodeCoverage>]
type IngestionActor(config: AppConfiguration, http: IInternalHttpClient) as self =

    let getStats inbox =
        async {
            let myStats = Actor.getStats $"{self.GetType()}" inbox
                        
            return myStats
        }

    let actor =
        MailboxProcessor<ActorMessage>.Start(fun inbox ->
            let rec loop () =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | ActorMessage.GetFeeds
                    | ActorMessage.AddFeed _
                    | ActorMessage.RemoveFeed _
                    | ActorMessage.FetchFeed _
                    | ActorMessage.Feeds _ -> ignore 0
                    | ActorMessage.IngestFeeds -> ignore 0
                    | ActorMessage.Documents _
                    | ActorMessage.IndexDoc _ -> ignore 0 // TODO:
                    | ActorMessage.GetActorStats rc -> inbox |> getStats |> Async.RunSynchronously |> rc.Reply // TODO: ewww
                    | m -> ignore 0

                    return! loop ()
                }

            loop ())

    interface IActor with
        member this.GetStats() =
            actor.PostAndAsyncReply(fun rc -> ActorMessage.GetActorStats rc)

        member this.Post(msg: ActorMessage) = actor.Post msg

        member this.ReplyAsync(msg: ActorMessage) = actor.PostAndAsyncReply(fun rc -> msg)
