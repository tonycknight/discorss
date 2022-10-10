namespace Discorss.Ingestion

open System.Diagnostics.CodeAnalysis
open Discorss

[<ExcludeFromCodeCoverage>]
type FeedActor(parent:IActor, config:AppConfiguration, http:IInternalHttpClient, feedUri)=

    let feedServiceUrl = $"{config.feedServiceUrl}/feeds/"

    let query() =
        task {
            let! r = $"{feedServiceUrl}{feedUri}/" |> http.GetAsync 
            return match r with
                    | HttpRequestResponse.HttpOkRequestResponse (_,body) -> Some body
                    | _ -> None
            }
    
    let toDocs(body:string)=
        Newtonsoft.Json.JsonConvert.DeserializeObject<Discorss.Indexing.Document[]>(body)
        
    let getDocuments()=
        task {
            let! r = query() 
            return r |> Option.map (fun x -> toDocs x |> ActorMessage.Documents)
        }

    let actor = MailboxProcessor<ActorMessage>.Start(
            fun inbox ->
                let rec loop() = async {

                    match! inbox.Receive() with
                    | ActorMessage.QueryFeed uri
                        when feedUri = uri ->       let! m = getDocuments() |> Async.AwaitTask
                                                    match m with
                                                    | Some m -> parent.Post m
                                                    | _ -> ignore 0
                    | m ->                          parent.Post m

                    return! loop()
                    }
                loop()    
            )
    
    interface IActor with
        member this.Post(msg: ActorMessage) = actor.Post msg
        member this.Start() = actor.Start()


