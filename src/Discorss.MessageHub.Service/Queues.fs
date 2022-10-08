namespace Discorss.MessageHub.Service

open System
open System.Threading.Tasks
open Discorss.Messaging

type IQueue=
    abstract member GetNextAsync : unit -> Task<MessageHubMessage option>
    abstract member PushAsync : message:MessageHubMessage -> Task

type MemoryQueue()=

    let queue = new System.Collections.Concurrent.ConcurrentQueue<MessageHubMessage>()
    
    interface IQueue with
        member this.GetNextAsync() =
            task {
                return match queue.TryDequeue() with
                        | true, msg -> Some msg
                        | false, _ -> None
            }

        member this.PushAsync message =
            task {
                queue.Enqueue message                
            }
    
