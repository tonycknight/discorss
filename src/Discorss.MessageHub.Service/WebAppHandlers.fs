namespace Discorss.MessageHub.Service

open System
open Discorss
open Discorss.Messaging
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers=
    let qp (sp:IServiceProvider) = sp.GetRequiredService<IQueueProvider>()
    
    let private getRequestedMessage (ctx : HttpContext)  =
        task {
            if ctx.Request.ContentType <> "application/json" then
                let result = { ApiErrorResult.errors = [| "Invalid content type" |] }
                return Choice1Of2 result
            else
                let! msg = ctx.BindModelAsync<MessageHubMessage>()
                // TODO: get the request from the payload; 400 if no good
                // invalid content type
                // invalid schema
                // missing content of any kind

                return Choice2Of2 msg
            }

    let pushMessage sp queueName =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                                      
                let qp = qp sp
                let! msg = getRequestedMessage ctx
                match msg with
                | Choice1Of2 error ->   return! RequestErrors.BAD_REQUEST error next ctx
                | Choice2Of2 msg ->     do! qp.PushAsync queueName msg
                                        return! Successful.NO_CONTENT next ctx
            }
    
    let getQueueNames sp =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                                      
                let qp = qp sp

                let! names = qp.GetQueuesAsync()

                return! Successful.OK names next ctx
            }

    let getNextMessage sp queueName =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                                      
                let qp = qp sp
                
                let! msg = qp.GetNextAsync queueName
                let resp = match msg with
                            | None -> Successful.NO_CONTENT
                            | Some m -> Successful.OK m

                return! resp next ctx 
            }
        

