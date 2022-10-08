namespace Discorss.MessageHub.Service

open System
open Discorss
open Discorss.Messaging
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers=
    let qp (sp:IServiceProvider) = sp.GetRequiredService<IQueueProvider>()
        
    let pushMessage sp queueName =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                                      
                match! ApiValidation.getRequest<MessageHubMessage> ctx with
                | Choice1Of2 error ->   return! RequestErrors.BAD_REQUEST error next ctx
                | Choice2Of2 msg ->     let! q = (qp sp).GetQueueAsync queueName 
                                        do! q.PushAsync msg
                                        return! Successful.NO_CONTENT next ctx
            }
    
    let getQueueNames sp =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                                                      
                let! names = (qp sp).GetQueuesAsync()

                return! Successful.OK names next ctx
            }

    let getNextMessage sp queueName =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                
                let! q = (qp sp).GetQueueAsync(queueName)
                let! msg = q.GetNextAsync()
                let resp = match msg with
                            | None -> Successful.NO_CONTENT
                            | Some m -> Successful.OK m

                return! resp next ctx 
            }
        

