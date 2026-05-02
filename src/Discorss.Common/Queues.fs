namespace Discorss.Queues

open System
open System.Threading.Tasks
open Discorss.Messaging
open Microbroker.Client

module QueueNames =
    
    [<Literal>]
    let feedEntries = "discorss_feedentries"

    [<Literal>]
    let feedIngest = "discorss_feedingest"

module Messages =
    let toQueueMessage (value: 'a) =
        { MicrobrokerMessage.messageType = value.GetType().AssemblyQualifiedName
          content = Newtonsoft.Json.JsonConvert.SerializeObject value
          created = DateTimeOffset.UtcNow
          active = DateTimeOffset.UtcNow
          expiry = DateTimeOffset.MaxValue }

    let fromQueueMessage<'a> (msg: MicrobrokerMessage) =
        if msg.messageType = typeof<'a>.AssemblyQualifiedName then
            msg.content |> Newtonsoft.Json.JsonConvert.DeserializeObject<'a> |> Some
        else
            None

type QueueInfo = { name: string; count: int }

type IQueue =
    abstract member GetNextAsync: unit -> Task<QueueMessage option>
    abstract member PushAsync: message: QueueMessage -> Task
    abstract member GetInfoAsync: unit -> Task<QueueInfo>

type MemoryQueue(name: string) =

    let queue = new System.Collections.Concurrent.ConcurrentQueue<QueueMessage>()

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
