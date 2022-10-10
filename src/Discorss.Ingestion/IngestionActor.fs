namespace Discorss.Ingestion

open System.Diagnostics.CodeAnalysis
open Discorss

[<ExcludeFromCodeCoverage>]
type IngestionActor(config:AppConfiguration, http:IInternalHttpClient) as self=

    let feedsActor = new FeedsActor(self, config, http) :> IActor

    let actor = MailboxProcessor<ActorMessage>.Start(
            fun inbox ->
                let rec loop() = async {
                    let! msg = inbox.Receive()
                    match msg with                    
                    | ActorMessage.GetFeeds 
                    | ActorMessage.AddFeed _
                    | ActorMessage.RemoveFeed _
                    | ActorMessage.QueryFeed _
                    | ActorMessage.Feeds _ ->       msg |> feedsActor.Post 
                    | ActorMessage.Documents _ ->   ignore 0 // TODO:
                    | ActorMessage.IndexDoc _ ->    ignore 0 // TODO: 
                    | m ->                          ignore 0

                    return! loop()
                    }
                loop()    
            )

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg
        member this.Start() = actor.Start()


