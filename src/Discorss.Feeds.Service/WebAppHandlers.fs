namespace Discorss.Feeds.Service

open System
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

    let previewFeed (sp: IServiceProvider)=
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {                
                let! articleRequest = ctx.BindJsonAsync<PreviewFeedRequest>()
                
                if String.IsNullOrWhiteSpace(articleRequest.uri) then
                    return! RequestErrors.BAD_REQUEST [] next ctx
                else
                    let hc = hc sp
                    let! feed = articleRequest.uri |> Discorss.Feeds.FeedReader.readAsync hc
                        
                    match feed with
                    | Discorss.Feeds.FeedReadResult.Feed feed ->    
                        let result = { PreviewFeedResponse.feed  = Some feed; uri = articleRequest.uri; messages = [] }
                        return! Successful.OK result next ctx
                    | Discorss.Feeds.FeedReadResult.Error msg ->    
                        let result = { PreviewFeedResponse.feed  = None; uri = articleRequest.uri; messages = [ msg ] }
                        return! RequestErrors.UNPROCESSABLE_ENTITY result next ctx
                    | _ ->        
                        let result = { PreviewFeedResponse.feed  = None; uri = articleRequest.uri; messages = [ "Internal error" ] }
                        return! RequestErrors.UNPROCESSABLE_ENTITY result next ctx
            }


