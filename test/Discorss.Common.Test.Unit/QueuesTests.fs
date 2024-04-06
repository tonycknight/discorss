namespace Discorss.Queues.Test.Unit

open System
open Discorss.Queues
open Discorss.Messaging
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit


module QueueMessageTests =

    [<Property>]
    let ``MemoryQueue push/get returns same msg`` (content: string) =

        let msg =
            { QueueMessage.empty () with
                content = content
                priority = 0.m }

        let q = new MemoryQueue("test") :> IQueue

        q.PushAsync(msg).ConfigureAwait(true) |> ignore

        let msg2 = q.GetNextAsync().Result

        match msg2 with
        | Some m -> m.content = msg.content && m.id = msg.id
        | _ -> false

    [<Property(MaxTest = 1)>]
    let ``MemoryQueue get returns None`` () =

        let q = new MemoryQueue("test") :> IQueue

        let msg2 = q.GetNextAsync().Result

        msg2 = None
        

    [<Property>]
    let ``MemoryQueue push repeated get eventually returns None`` (content: string) =
        let msg =
            { QueueMessage.empty () with
                content = content
                priority = 0.m }

        let q = new MemoryQueue("test") :> IQueue

        q.PushAsync(msg).ConfigureAwait(true) |> ignore

        let msg2 = q.GetNextAsync().Result
        let msg3 = q.GetNextAsync().Result

        match msg2, msg3 with
        | Some m, None -> m.content = msg.content && m.id = msg.id
        | _ -> false


    [<Property>]
    let ``MemoryQueue GetInfoAsync returns msg count`` (count: PositiveInt) =
        let q = new MemoryQueue("test") :> IQueue

        let msgs =
            [ 1 .. count.Get ]
            |> List.map (fun _ ->
                { QueueMessage.empty () with
                    content = "content"
                    priority = 0.m })

        msgs |> List.map (fun msg -> q.PushAsync(msg).ConfigureAwait(true)) |> ignore

        let info = q.GetInfoAsync().Result

        info.count = count.Get
