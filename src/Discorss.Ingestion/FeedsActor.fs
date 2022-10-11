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
                    let state = match msg with
                                | ActorMessage.GetFeeds ->          queryFeeds() |> Async.RunSynchronously // TODO: ewwww
                                                                        |> Option.iter (ActorMessage.Feeds >> parent.Post)
                                                                    state
                                | ActorMessage.Feeds feeds ->       addFeeds state feeds
                                | ActorMessage.AddFeed uri ->       let state,_ = getOrCreateFeed state uri
                                                                    state
                                | ActorMessage.RemoveFeed uri ->    removeFeed state uri                                                                    
                                | ActorMessage.QueryFeed uri ->     let state,actor = getOrCreateFeed state uri
                                                                    msg |> actor.Post
                                                                    state 
                                | m ->                              parent.Post m
                                                                    state

                    return! loop state
                    }
                                    
                state() |> loop
            )

    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg
        
            

