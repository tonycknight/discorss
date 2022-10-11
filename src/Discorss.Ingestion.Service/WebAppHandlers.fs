namespace Discorss.Ingestion.Service

open System
open Discorss
open Discorss.Ingestion
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers=
    
    let actor (sp: IServiceProvider)= sp.GetRequiredService<Ingestion.IngestionActor>() :> IActor
    
    let testIngestion(sp:IServiceProvider)=
        
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                                      
                // TODO: placeholder
                let actor = actor sp

                ActorMessage.GetFeeds |> actor.Post
                

                return! Successful.OK [] next ctx
            }


