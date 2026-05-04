namespace Discorss.Documents

open System
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Logging

type IDocumentNotificationWriter =
    abstract member SetAsync: Document -> Task

type DocumentNotificationWriter(logFactory: ILoggerFactory, broker: Microbroker.Client.IMicrobrokerProxy) =
    let log = logFactory.CreateLogger<DocumentNotificationWriter>()

    interface IDocumentNotificationWriter with

        member this.SetAsync(document: Document) =
            task {
                let m = document.uri |> Queues.Messages.toRawMessage
                do! broker.PostAsync (Queues.QueueNames.documentNotifications, m)
            }
