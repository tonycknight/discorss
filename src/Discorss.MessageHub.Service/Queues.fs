namespace Discorss.MessageHub.Service

open System
open System.Threading.Tasks
open Discorss.Messaging

type QueueInfo = { name: string; count: int }

type IQueue =
    abstract member GetNextAsync: unit -> Task<MessageHubMessage option>
    abstract member PushAsync: message: MessageHubMessage -> Task
    abstract member GetInfoAsync: unit -> Task<QueueInfo>

type MemoryQueue(name: string) =

    let queue = new System.Collections.Concurrent.ConcurrentQueue<MessageHubMessage>()

    interface IQueue with
        member this.GetInfoAsync() =
            task {
                return
                    { QueueInfo.name = name
                      count = queue.Count }
            }

        member this.GetNextAsync() =
            task {
                return
                    match queue.TryDequeue() with
                    | true, msg -> Some msg
                    | false, _ -> None
            }

        member this.PushAsync message = task { queue.Enqueue message }
