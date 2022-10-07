namespace Discorss.Ingestion.Service

open System
open Discorss
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers=
    

    let getData (sp: IServiceProvider)=
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                                      
                // TODO: placeholder
                return! Successful.OK [] next ctx
            }
        

