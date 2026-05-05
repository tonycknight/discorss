namespace Discorss.Documents.Service

open System
open Discorss
open Discorss.ApiModels
open Discorss.Documents
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers =
    let getNextDocument (sp: IServiceProvider) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let reader = sp.GetRequiredService<IDocumentNotificationReader>()

                let! doc = reader.GetNextAsync()

                match doc with
                | None -> return! Successful.NO_CONTENT next ctx
                | Some doc ->
                    // TODO: translate to ApiModel
                    return! Successful.ok (json doc) next ctx
            }
