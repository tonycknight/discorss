namespace Discorss.Queues

open System
open System.Threading.Tasks
open Discorss.Messaging
open Microbroker.Client

module QueueNames =
    
    [<Literal>]
    let feedEntries = "discorss_feedentries"

module Messages =
    let toQueueMessage (value: 'a) =
        { MicrobrokerMessage.messageType = value.GetType().AssemblyQualifiedName
          content = Newtonsoft.Json.JsonConvert.SerializeObject value
          created = DateTimeOffset.UtcNow
          active = DateTimeOffset.UtcNow
          expiry = DateTimeOffset.MaxValue }

    let fromQueueMessage<'a> (msg: MicrobrokerMessage) =
        try
            msg.content |> Newtonsoft.Json.JsonConvert.DeserializeObject<'a> |> Some
        with
        | :? Newtonsoft.Json.JsonReaderException as ex -> None
        | :? Newtonsoft.Json.JsonSerializationException as ex -> None
