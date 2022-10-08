namespace Discorss.MessageHub.Service

open System
open Discorss.Messaging
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers=
    let qp (sp:IServiceProvider) = sp.GetRequiredService<IQueueProvider>()
    let msg (ctx : HttpContext) = ctx.BindModelAsync<MessageHubMessage>()

    let pushMessage (sp:IServiceProvider) queueName =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                                      
                let qp = qp sp
                let! msg = ctx.BindModelAsync<MessageHubMessage>()

                do! qp.PushAsync queueName msg

                return! Successful.NO_CONTENT next ctx
            }
    
    let getQueueNames (sp:IServiceProvider) =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                                      
                let qp = qp sp

                let! names = qp.GetQueueNamesAsync()

                return! Successful.OK names next ctx
            }

    let getNextMessage (sp:IServiceProvider) queueName =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                                      
                let qp = qp sp
                
                let! msg = qp.GetNextAsync queueName
                let resp = match msg with
                            | None -> Successful.NO_CONTENT
                            | Some m -> Successful.OK m

                return! resp next ctx 
            }
        

