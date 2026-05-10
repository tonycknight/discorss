namespace Discorss

open System
open System.Net
open System.Net.Http
open System.Threading
open Discorss.ApiModels

module DiscorssApi =

    type OnOkResponse<'a> = (HttpStatusCode * string * string option * HttpResponseHeaders) -> 'a

    let private send req =
        task {
            let client = Http.client ()

            return! req |> Http.send CancellationToken.None client
        }

    let private onResponse (ok: OnOkResponse<'a>) (resp: HttpRequestResponse) =
        match resp with
        | HttpBadGatewayResponse _
        | HttpTooManyRequestsResponse _ -> new Exception($"{resp.GetType().Name} received.") |> raise
        | HttpErrorRequestResponse(status, body, headers, errors) -> new Exception($"{status} received.") |> raise
        | HttpExceptionRequestResponse ex -> raise ex
        | HttpOkRequestResponse(status, body, contentType, headers) -> ok (status, body, contentType, headers)

    let getFeeds host =
        task {
            let uri = Http.route host "api/v1/feeds/"

            let req = new HttpRequestMessage(HttpMethod.Get, uri)

            let! resp = send req

            return
                resp
                |> onResponse (fun (_, body, _, _) ->
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Discorss.ApiModels.FeedInfo[]> body)
        }

    let previewFeeds host feedUri =
        task {
            let uri = Http.route host $"api/v1/feeds/{Http.encode feedUri}/"

            let req = new HttpRequestMessage(HttpMethod.Get, uri)

            let! resp = send req

            return
                resp
                |> onResponse (fun (_, body, _, _) ->
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Discorss.ApiModels.Feed> body)
        }

    let addFeeds host (feed: ApiModels.FeedInfo) =
        task {

            let uri = Http.route host "api/v1/feeds/"

            let body = feed |> Newtonsoft.Json.JsonConvert.SerializeObject

            let req = new HttpRequestMessage(HttpMethod.Put, uri) |> Http.applyJsonContent body

            let! resp = send req

            return
                resp
                |> onResponse (fun (_, body, _, _) ->
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Discorss.ApiModels.FeedInfo> body)
        }
