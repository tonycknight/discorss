namespace Discorss.Documents

open System
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Logging

type IDocumentNotificationReader =
    abstract member GetNextAsync: unit -> Task<Document option>

type DocumentNotificationReader(logFactory: ILoggerFactory, docRepo: IDocumentRepository, broker: Microbroker.Client.IMicrobrokerProxy) =
    
    let log = logFactory.CreateLogger<DocumentNotificationReader>()

    let getNextDocument () =
        task {
            let! msg = broker.GetNextAsync Queues.QueueNames.documentNotifications

            return!
                match msg with
                | None -> task { return None }
                | Some msg -> docRepo.GetDocumentAsync msg.content
        }

    interface IDocumentNotificationReader with
        member this.GetNextAsync () = getNextDocument ()