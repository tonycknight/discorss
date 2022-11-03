namespace Discorss.Ingestion

open System.Diagnostics.CodeAnalysis
open Discorss

type FeedsActorState = {
    feedActors: Map<string, IActor>
    }

[<ExcludeFromCodeCoverage>]
type FeedsActor(parent:IActor, config:AppConfiguration, http:IInternalHttpClient) as self=

    let feedServiceUrl = $"{config.feedServiceUrl}/api/v1/feeds/"    
    let state() = { feedActors = Map.empty } 

    let getStats inbox state = 
        async {
            let myStats = Actor.getStats $"{self.GetType()}" inbox

            let! feedStats = 
                      state.feedActors  |> Seq.map (fun kv -> kv.Value)
                                        |> Seq.map (fun a -> a.GetStats())
                                        |> Array.ofSeq
                                        |> Async.Parallel
                                                    
            return { myStats with childStats = feedStats |> List.ofArray }
        }


    let queryFeeds()=
        async {
            let! r = feedServiceUrl |> http.GetAsync |> Async.AwaitTask
            return match r with
                    | HttpRequestResponse.HttpOkRequestResponse (_,body) -> body |> Newtonsoft.Json.JsonConvert.DeserializeObject<Feeds.FeedInfo[]> |> Some
                    | _ -> None
        }

    let setFeeds(feeds: Feeds.FeedInfo[] option)=
        feeds |> Option.bind (ActorMessage.Feeds >> Some)

    let getOrCreateFeed (state:FeedsActorState) uri =
        let state,actor = 
                    match state.feedActors.TryFind uri with
                    | Some feed ->  (state,feed)
                    | _ ->          let actor = new FeedActor(self, config, http, uri) :> IActor
                                    let state = { state with feedActors = state.feedActors |> Map.add uri actor }
                                    (state, actor)

        (state,actor)

    let addFeeds (state:FeedsActorState) (feeds: Feeds.FeedInfo[]) =
        let mutable state = state

        for feed in feeds do  
            let s,_ = getOrCreateFeed state feed.uri
            state <- s

        state

    let removeFeed (state:FeedsActorState) uri =
        { state with feedActors = state.feedActors |> Map.remove uri }
        
    let actor = MailboxProcessor<ActorMessage>.Start(
            fun inbox ->
                let rec loop(state:FeedsActorState) = async {
                    let! msg = inbox.Receive()
                    let! state = match msg with
                                    | ActorMessage.GetFeeds ->          async {
                                                                                let! feeds = queryFeeds() 
                                                                                feeds |> Option.iter (ActorMessage.Feeds >> parent.Post)
                                                                                return state
                                                                            }
                                    | ActorMessage.Feeds feeds ->       async { return addFeeds state feeds }
                                    | ActorMessage.AddFeed uri ->       async {
                                                                                let state,_ = getOrCreateFeed state uri
                                                                                return state
                                                                            }
                                    | ActorMessage.RemoveFeed uri ->    async { return removeFeed state uri }
                                    | ActorMessage.FetchFeed uri ->     async {
                                                                                let state,actor = getOrCreateFeed state uri
                                                                                msg |> actor.Post
                                                                                return state 
                                                                            }
                                    | ActorMessage.QueryFeeds rc ->     async {
                                                                            state.feedActors |> Seq.map (fun kv -> kv.Key) 
                                                                            |> Array.ofSeq
                                                                            |> rc.Reply
                                                                            return state
                                                                        }
                                    | ActorMessage.GetActorStats rc->   async {
                                                                            getStats inbox state |> Async.RunSynchronously |> rc.Reply 
                                                                            return state
                                                                        }
                                    | m ->                              async {
                                                                            parent.Post m
                                                                            return state
                                                                        }

                    return! loop state
                    }
                                    
                state() |> loop
            )

    interface IActor with
        member this.Post(msg:ActorMessage) = actor.Post msg
        member this.GetStats() = actor.PostAndAsyncReply (fun rc -> ActorMessage.GetActorStats rc)        
        member this.ReplyAsync(msg:ActorMessage) = actor.PostAndAsyncReply (fun rc -> msg)
        
            

