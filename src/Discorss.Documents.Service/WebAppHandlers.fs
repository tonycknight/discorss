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
                | Some doc -> return! Successful.ok (doc |> Mapping.toDocumentApiModel |> json) next ctx
            }

    let deleteDocumentLike (sp: IServiceProvider) (uri: string) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let repo = sp.GetRequiredService<IDocumentLikeRepository>()

                do! repo.DeleteAsync uri

                return! Successful.NO_CONTENT next ctx
            }

    let getDocumentLike (sp: IServiceProvider) (uri: string) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let repo = sp.GetRequiredService<IDocumentLikeRepository>()

                let! doc = repo.GetAsync uri

                match doc with
                | None ->
                    let result = json { ApiErrorResult.errors = [| "Not found." |] }
                    return! RequestErrors.notFound result next ctx
                | Some doc -> return! Successful.ok (doc |> Mapping.toDocumentLikeApiModel |> json) next ctx
            }

    let setDocumentLike (sp: IServiceProvider) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {

                let! req = Api.getRequest<ApiModels.DocumentLike> ctx

                match req with
                | Choice1Of2 error ->
                    let result = json { ApiErrorResult.errors = [| error |] }
                    return! RequestErrors.badRequest result next ctx
                | Choice2Of2 doc ->
                    let repo = sp.GetRequiredService<IDocumentLikeRepository>()

                    let! r = doc |> Mapping.fromDocumentLikeApiModel |> repo.SetAsync

                    return! Successful.ok (r |> Mapping.toDocumentLikeApiModel |> json) next ctx
            }
