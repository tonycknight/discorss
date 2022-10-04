namespace Discorss.Feeds.Service

open System
open Discorss
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers=
    let private hc (sp: IServiceProvider) = sp.GetRequiredService<Discorss.IExternalHttpClient>()
    let private feedRepo (sp: IServiceProvider) = sp.GetRequiredService<Discorss.Feeds.IFeedRepository>()
        
    let getFeeds (sp: IServiceProvider)=
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                
                let! feeds = (feedRepo sp).GetFeedsAsync()
                    
                return! Successful.OK feeds next ctx
            }

    let getFeed (sp: IServiceProvider) feedUri=
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                
                let hc = hc sp
                
                // TODO: check cache

                let! feed = feedUri |> Discorss.Feeds.FeedReader.readAsync hc
                
                match feed with
                    | Discorss.Feeds.FeedReadResult.Feed feed ->                            
                        return! Successful.OK feed next ctx
                    | Discorss.Feeds.FeedReadResult.Error msg ->    
                        let result = { ApiErrorResult.errors = [| msg|]}
                        return! RequestErrors.UNPROCESSABLE_ENTITY result next ctx
                    | _ ->  
                        let result = { ApiErrorResult.errors = [| "Internal error" |]}
                        return! RequestErrors.UNPROCESSABLE_ENTITY result next ctx

            }


