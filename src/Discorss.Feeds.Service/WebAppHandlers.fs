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

                let result = feeds |> Seq.map Mapping.toFeedInfoApiModel |> Array.ofSeq |> json
                
                return! Successful.ok result next ctx
            }

    let getFeed (sp: IServiceProvider) feedUri =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                if Uri.tryParse feedUri |> Option.isNone then
                    let result = json { ApiErrorResult.errors = [| "Invalid Uri" |] }
                    return! RequestErrors.BAD_REQUEST result next ctx
                else
                    let! feed = (feedProvider sp).GetFeedAsync feedUri

                    match feed with
                    | FeedReadResult.Feed feed ->       
                        let result = feed |> Mapping.toFeedApiModel |> json 
                        
                        return! Successful.ok result next ctx 

                    | FeedReadResult.Error msg ->
                        let result = json { ApiErrorResult.errors = [| msg |] }
                        return! RequestErrors.unprocessableEntity result next ctx
                    | _ ->
                        let result = json { ApiErrorResult.errors = [| "Internal error" |] }
                        return! ServerErrors.internalError result next ctx                        

            }
