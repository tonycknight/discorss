namespace Discorss.Documents

open System
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Logging

type IDocumentNotificationReader =
    abstract member GetNextAsync: unit -> Task<Document option>

type DocumentNotificationReader
    (logFactory: ILoggerFactory, docRepo: IDocumentRepository, broker: Microbroker.Client.IMicrobrokerProxy) =

    let log = logFactory.CreateLogger<DocumentNotificationReader>()

    let rec getNext () =
        task {
            log.LogTrace "Fetching next document notification..."
            let! msg = broker.GetNextAsync (Queues.QueueNames.documentNotifications, TimeSpan.FromSeconds 1.)

            return!
                match msg with
                | None ->
                    task {
                        log.LogTrace "No notification found."
                        return None
                    }
                | Some msg ->
                    task {
                        log.LogTrace $"Finding documnet {msg.content}..."
                        let! doc = docRepo.GetDocumentAsync msg.content

                        return!
                            match doc with
                            | Some doc ->
                                task {
                                    log.LogTrace $"Found document {doc.uri}."
                                    return Some doc
                                }
                            | _ ->
                                log.LogTrace $"Could not find documnet {msg.content}, retrying..."
                                getNext ()
                    }
        }

    interface IStatsSource with
        member this.GetStatsAsync() =
            task {
                let! mbCount = broker.GetQueueCountAsync Queues.QueueNames.documentNotifications

                return
                    { Stats.name = this.GetType().Name
                      itemCount = mbCount |> Option.map _.count |> Option.defaultValue 0L
                      childStats = [] }
            }


    interface IDocumentNotificationReader with
        member this.GetNextAsync() = getNext ()
