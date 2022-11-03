namespace Discorss.Ingestion

open System.Diagnostics.CodeAnalysis
open Discorss

[<ExcludeFromCodeCoverage>]
type FeedActor(parent:IActor, config:AppConfiguration, http:IInternalHttpClient, feedUri) as self=

    let feedServiceUrl = $"{config.feedServiceUrl}/api/v1/feeds/"

    let queryFeed() =
        task {
            let! r = $"{feedServiceUrl}{feedUri}/" |> http.GetAsync 
            return match r with
                    | HttpRequestResponse.HttpOkRequestResponse (_,body) -> Some body
                    | _ -> None
            }
    
    let toDocs(body:string)=
        Newtonsoft.Json.JsonConvert.DeserializeObject<Discorss.Indexing.Document[]>(body)
        
    let getFeedDocuments()=
        task {
            let! r = queryFeed() 
            return r |> Option.map (toDocs >> ActorMessage.Documents)
        }

    let actor = MailboxProcessor<ActorMessage>.Start(
            fun inbox ->
                let rec loop() = async {

                    match! inbox.Receive() with
                    | ActorMessage.FetchFeed uri
                        when feedUri = uri ->       
                                                        let! m = getFeedDocuments() |> Async.AwaitTask
                                                        m |> Option.iter parent.Post
                    | ActorMessage.GetActorStats rc->   inbox |> Actor.getStats $"{self.GetType()} - {feedUri}" |> rc.Reply 
                    | m ->                              parent.Post m

                    return! loop()
                    }
                loop()    
            )
    
    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg        
        member this.GetStats() = actor.PostAndAsyncReply (fun rc -> ActorMessage.GetActorStats rc)        
        member this.ReplyAsync(msg:ActorMessage) = actor.PostAndAsyncReply (fun rc -> msg)
