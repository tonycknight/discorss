namespace Discorss.Indexing.Service

open System
open Discorss
open Discorss.Indexing
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers =

    let getDocumentWords (sp: IServiceProvider) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                match! ApiValidation.getRequest<ArticleRequest> ctx with
                | Choice1Of2 error -> return! RequestErrors.BAD_REQUEST error next ctx
                | Choice2Of2 req ->
                    let da = sp.GetRequiredService<Indexing.IDocumentAnalyser>()

                    let words =
                        { Document.uri = req.uri
                          title = req.title
                          description = req.description
                          content = req.content
                          author = req.author }
                        |> da.Words

                    return! Successful.OK words next ctx

            }

    let getDocumentStatistics (sp: IServiceProvider) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                match! ApiValidation.getRequest<ArticleRequest> ctx with
                | Choice1Of2 error -> return! RequestErrors.BAD_REQUEST error next ctx
                | Choice2Of2 req ->
                    let da = sp.GetRequiredService<Indexing.IDocumentAnalyser>()

                    let doc =
                        { Document.uri = req.uri
                          title = req.title
                          description = req.description
                          content = req.content
                          author = req.author }

                    let stats = da.Statistics doc

                    return! Successful.OK stats next ctx
            }

    let setDocument (sp: IServiceProvider) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                match! ApiValidation.getRequest<ArticleRequest> ctx with
                | Choice1Of2 error -> return! RequestErrors.BAD_REQUEST error next ctx
                | Choice2Of2 req ->
                    let doc =
                        { Document.uri = req.uri
                          title = req.title
                          description = req.description
                          content = req.content
                          author = req.author }

                    let stats = doc |> sp.GetRequiredService<Indexing.IDocumentAnalyser>().Statistics

                    do! sp.GetRequiredService<Indexing.IDocumentStatsWriter>().Set(stats)

                    return! Successful.NO_CONTENT next ctx
            }
