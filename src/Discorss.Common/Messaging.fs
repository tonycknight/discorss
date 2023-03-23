namespace Discorss.Messaging

open System
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks
open Discorss

type MessageHubMessage =
    { id: Guid
      priority: decimal
      messageType: string
      content: string
      created: DateTimeOffset
      ttl: DateTimeOffset option }

    static member empty() =
        { MessageHubMessage.id = Guid.NewGuid()
          priority = 0M
          messageType = ""
          content = null
          created = DateTimeOffset.UtcNow
          ttl = None }


type IMessageHubClient =
    abstract member GetNextAsync: queueName: string -> Task<MessageHubMessage option>
    abstract member PushAsync: queueName: string -> msg: MessageHubMessage -> Task

[<ExcludeFromCodeCoverage>]
type MessageHubClient(config: AppConfiguration, client: IInternalHttpClient) =
    let serviceConfig = config.messageHubServiceUrl

    let getNextMessage queueName =
        task {
            let! resp = client.GetAsync $"{serviceConfig}/api/v1/queue/{queueName}/head/"

            return
                match resp with
                | HttpOkRequestResponse(status, body) when status = System.Net.HttpStatusCode.OK ->
                    body |> Newtonsoft.Json.JsonConvert.DeserializeObject<MessageHubMessage> |> Some
                | _ -> None
        }

    let pushMessage queueName msg =
        task {
            let! resp =
                msg
                |> Newtonsoft.Json.JsonConvert.SerializeObject
                |> client.PostAsync $"{serviceConfig}/api/v1/queue/{queueName}/"

            match resp with
            | HttpExceptionRequestResponse ex -> Exception($"Cannot send message", ex) |> raise
            | HttpErrorRequestResponse(_, msg) -> Exception($"Cannot send message{Environment.NewLine}{msg}") |> raise
            | _ -> ignore 0
        }

    interface IMessageHubClient with
        member this.GetNextAsync queueName = getNextMessage queueName
        member this.PushAsync queueName msg = pushMessage queueName msg
