namespace Discorss.MessageHub.Service

open System
open System.Threading.Tasks
open Discorss.Messaging

type IQueueProvider =
    abstract member GetQueuesAsync : unit -> Task<QueueInfo[]>
    abstract member GetNextAsync : queueName:string -> Task<MessageHubMessage option>
    abstract member PushAsync : queueName:string -> message:MessageHubMessage -> Task

type QueueProvider()=
    
    let queues = new System.Collections.Concurrent.ConcurrentDictionary<string, IQueue>(StringComparer.OrdinalIgnoreCase)

    let getQueue queueName =
        queues.GetOrAdd(queueName, (fun n -> new MemoryQueue(n) :> IQueue ) )
        
    interface IQueueProvider with
        member this.GetQueuesAsync() = 
            task {
                let qis = queues.Values
                                    |> Array.ofSeq
                                    |> Array.Parallel.map (fun q -> q.GetInfoAsync())                                    
                return! Task.WhenAll qis                
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
    