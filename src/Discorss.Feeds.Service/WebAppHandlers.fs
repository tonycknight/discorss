namespace Discorss.Feeds.Service

open System
open Discorss
open Discorss.Feeds
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers =
    let private hc (sp: IServiceProvider) =
        sp.GetRequiredService<Discorss.IExternalHttpClient>()

    let private feedRepo (sp: IServiceProvider) =
        sp.GetRequiredService<IFeedRepository>()

    let private feedProvider (sp: IServiceProvider) = sp.GetRequiredService<IFeedProvider>()
    
    let getFeeds (sp: IServiceProvider) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! feeds = (feedRepo sp).GetFeedInfosAsync()

                let result = feeds |> Seq.map Mapping.toFeedInfoApiModel |> Array.ofSeq // TODO: |> json
                
                return! Successful.OK result next ctx
            }

    let getFeed (sp: IServiceProvider) feedUri =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                if Uri.tryParse feedUri |> Option.isNone then
                    let result = { ApiErrorResult.errors = [| "Invalid Uri" |] }
                    return! RequestErrors.BAD_REQUEST result next ctx
                else
                    let! feed = (feedProvider sp).GetFeedAsync feedUri

                    match feed with
                    | FeedReadResult.Feed feed ->       
                        let result = Mapping.toFeedApiModel feed
                        return! Successful.OK result next ctx // TODO: need to map to an ApiModel as json, causes "FSharp.Core v11 not found".... 
                    | FeedReadResult.Error msg ->
                        let result = json { ApiErrorResult.errors = [| msg |] }
                        return! RequestErrors.UNPROCESSABLE_ENTITY result next ctx
                    | _ ->
                        let result = json { ApiErrorResult.errors = [| "Internal error" |] }
                        return! ServerErrors.INTERNAL_ERROR result next ctx                        

            }
