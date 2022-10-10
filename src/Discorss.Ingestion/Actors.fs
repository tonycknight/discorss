namespace Discorss.Ingestion

open System.Threading.Tasks

module Actors =
    
    let orchActor() = MailboxProcessor<ActorMessage>.Start(
            fun inbox ->
                let rec loop() = async {

                    match! inbox.Receive() with
                    | ActorMessage.Start -> ignore 0
                    | ActorMessage.Stop -> ignore 0
                    | _ -> ignore 0

                    return! loop()
                    }
                loop()    
            )
    
    let receiveFeedsActor() = MailboxProcessor<ActorMessage>.Start(
            fun inbox ->
                let rec loop() = async {

                    match! inbox.Receive() with
                    | ActorMessage.Feeds urls ->    ignore 0 // TODO: 
                    | _ ->                          ignore 0

                    return! loop()
                    }
                loop()    
            )
    

    let receiveDocActor() = MailboxProcessor<ActorMessage>.Start(
            fun inbox ->
                let rec loop() = async {

                    match! inbox.Receive() with
                    | ActorMessage.Documents docs ->ignore 0 // TODO: 
                    | _ ->                          ignore 0

                    return! loop()
                    }
                loop()    
            )

type IActor =
    abstract member Start : unit -> unit
    abstract member Post : ActorMessage -> unit
    
type Actor(actors: MailboxProcessor<ActorMessage>[])= 

    interface IActor with
        member this.Post(msg: ActorMessage) = actors.[0].Post msg // TODO: round robin? 
        member this.Start() = actors |> Array.iter (fun a -> a.Start() )

