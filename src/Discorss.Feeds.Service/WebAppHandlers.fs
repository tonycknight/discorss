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

    let private feedInfo feedUri title =
        { FeedInfo.uri = feedUri
          title = title
          lastFetched = DateTimeOffset.UtcNow
          updated = DateTimeOffset.UtcNow }

    let getFeeds (sp: IServiceProvider) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! feeds = (feedRepo sp).GetFeedInfosAsync()

                let result = feeds |> Seq.map Mapping.toFeedInfoApiModel |> Array.ofSeq
                
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
                        let fi = feedInfo feedUri feed.title
                        do! (feedRepo sp).SetFeedInfoAsync fi

                        return! Successful.OK feed next ctx
                    | FeedReadResult.Error msg ->
                        let result = { ApiErrorResult.errors = [| msg |] }
                        return! RequestErrors.UNPROCESSABLE_ENTITY result next ctx
                    | _ ->
                        let result = { ApiErrorResult.errors = [| "Internal error" |] }
                        return! RequestErrors.UNPROCESSABLE_ENTITY result next ctx

            }
