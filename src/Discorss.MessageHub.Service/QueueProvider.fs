namespace Discorss.MessageHub.Service

open System
open System.Threading.Tasks
open Discorss.Messaging

type IQueueProvider =
    abstract member GetQueuesAsync : unit -> Task<QueueInfo[]>
    abstract member GetQueueAsync : queueName:string -> Task<IQueue>

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

        member this.GetQueueAsync(queueName) = 
            task {
                return getQueue queueName
            }        
    