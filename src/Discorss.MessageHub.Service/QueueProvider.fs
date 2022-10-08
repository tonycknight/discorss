namespace Discorss.MessageHub.Service

open System
open System.Threading.Tasks
open Discorss.Messaging

type IQueueProvider =
    abstract member GetQueueNamesAsync : unit -> Task<string[]>
    abstract member GetNextAsync : queueName:string -> Task<MessageHubMessage option>
    abstract member PushAsync : queueName:string -> message:MessageHubMessage -> Task

type QueueProvider()=
    
    let queues = new System.Collections.Concurrent.ConcurrentDictionary<string, IQueue>(StringComparer.OrdinalIgnoreCase)

    let getQueue queueName =
        queues.GetOrAdd(queueName, (fun _ ->  new MemoryQueue() :> IQueue ) )
        
    interface IQueueProvider with
        member this.GetQueueNamesAsync() = 
            task {
                return queues.Keys |> Seq.sort |> Array.ofSeq
            }

        member this.GetNextAsync(queueName) =
            task {
                let queue = getQueue queueName

                return! queue.GetNextAsync()
            }

        member this.PushAsync queueName message =
            task {                                
                let queue = getQueue queueName

                do! queue.PushAsync message
            }
    