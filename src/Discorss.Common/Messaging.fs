namespace Discorss.Messaging

open System
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks
open Discorss

type MessageHubMessage = {
    id:         Guid
    priority:   decimal
    }

type IMessageHubClient = 
    abstract member GetNextAsync : queueName:string -> Task<MessageHubMessage option>

[<ExcludeFromCodeCoverage>]
type MessageHubClient(config: Discorss.Configuration.IConfigurationProvider, client: IInternalHttpClient) =
        
    interface IMessageHubClient with

        member this.GetNextAsync(queueName)=
            task {
                
                return None // TODO: 
            }
