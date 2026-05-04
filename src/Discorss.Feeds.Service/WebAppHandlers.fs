namespace Discorss.Feeds.Service

open System
open Discorss
open Discorss.ApiModels
open Discorss.Feeds
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe

module WebAppHandlers =
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
                    return! RequestErrors.badRequest result next ctx
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
                        let result =
                            json { ApiErrorResult.errors = [| $"Internal error: {feed.GetType()}" |] }

                        return! ServerErrors.internalError result next ctx

            }

    let setFeed (sp: IServiceProvider) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {

                match! Api.getRequest<ApiModels.FeedInfo> ctx with
                | Choice1Of2 error ->
                    let result = json { ApiErrorResult.errors = [| error |] }
                    return! RequestErrors.badRequest result next ctx
                | Choice2Of2 req ->
                    try
                        match! (feedProvider sp).GetFeedAsync req.uri with
                        | FeedReadResult.Feed fr ->
                            let feed =
                                { FeedInfo.uri = req.uri
                                  title = fr.title
                                  description = fr.description
                                  updated = DateTime.UtcNow
                                  lastFetched = DateTime.MinValue }

                            let! result = (feedRepo sp).SetFeedInfoAsync feed

                            return! Successful.ok (result |> Mapping.toFeedInfoApiModel |> json) next ctx
                        | FeedReadResult.Xml _ ->
                            let result = json { ApiErrorResult.errors = [| "An error occurred." |] }
                            return! ServerErrors.internalError result next ctx
                        | FeedReadResult.Error err ->
                            let result = json { ApiErrorResult.errors = [| $"An error occurred: {err}" |] }
                            return! ServerErrors.internalError result next ctx
                    with ex ->
                        let result = json { ApiErrorResult.errors = [| "An error occurred." |] }
                        return! ServerErrors.internalError result next ctx
            }
