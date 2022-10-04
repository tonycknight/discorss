namespace Discorss.Indexing.Service

open System
open Discorss
open Discorss.Indexing
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers=
    

    let private validateArticle (ctx : HttpContext)  =
        task {
            if ctx.Request.ContentType <> "application/json" then
                let result = { ApiErrorResult.errors = [| "Invalid content type" |] }
                return Choice1Of2 result
            else
                let! req = ctx.BindModelAsync<ArticleRequest>()
                // TODO: get the request from the payload; 400 if no good
                // invalid content type?
                // invalid schema?
                // missing uri
                // missing content of any kind?

                return Choice2Of2 req
            }

    let getDocumentStatistics (sp: IServiceProvider)=
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                      
                match! validateArticle ctx with
                | Choice1Of2 error -> return! RequestErrors.BAD_REQUEST error next ctx
                | Choice2Of2 req ->                     
                    let da = sp.GetRequiredService<Indexing.IDocumentAnalyser>()
                    let doc = { Document.uri = req.uri; 
                                        title = req.title; 
                                        description = req.description; 
                                        content = req.content; 
                                        author = req.author }
                    let stats = da.Analyse doc

                    let freqs = stats.wordFrequencies |> Seq.sortByDescending snd

                    return! Successful.OK freqs next ctx
            }
        

