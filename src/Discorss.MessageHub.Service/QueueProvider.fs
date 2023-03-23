namespace Discorss.MessageHub.Service

open System
open System.Threading.Tasks

type IQueueFactory =
    abstract member CreateQueue: name: string -> IQueue

type MemoryQueueFactory() =
    interface IQueueFactory with
        member this.CreateQueue(name: string) = new MemoryQueue(name)

type IQueueProvider =
    abstract member GetQueuesAsync: unit -> Task<QueueInfo[]>
    abstract member GetQueueAsync: queueName: string -> Task<IQueue>

type QueueProvider(queueFactory: IQueueFactory) =

    let queues =
        new System.Collections.Concurrent.ConcurrentDictionary<string, IQueue>(StringComparer.OrdinalIgnoreCase)

    let getQueue queueName =
        queues.GetOrAdd(queueName, queueFactory.CreateQueue)

    interface IQueueProvider with
        member this.GetQueuesAsync() =
            task {
                let qis =
                    queues.Values |> Array.ofSeq |> Array.Parallel.map (fun q -> q.GetInfoAsync())

                return! Task.WhenAll qis
            }

        member this.GetQueueAsync(queueName) = task { return getQueue queueName }
