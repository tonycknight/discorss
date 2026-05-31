namespace Discorss

open System
open System.Net
open System.Net.Http
open System.Threading
open Discorss.ApiModels

module DiscorssApi =

    type OkResponseHandler<'a> = (HttpStatusCode * string * string option * HttpResponseHeaders) -> 'a
    type ErrorResponseHandler<'a> = (HttpStatusCode * string * HttpResponseHeaders * HttpResponseErrors) -> 'a

    let private send req =
        task {
            let client = Http.client ()

            return! req |> Http.send CancellationToken.None client
        }

    let private handleResponse
        (ok: OkResponseHandler<'a>)
        (error: ErrorResponseHandler<'a>)
        (resp: HttpRequestResponse)
        =
        match resp with
        | HttpBadGatewayResponse _
        | HttpTooManyRequestsResponse _ -> new Exception($"{resp.GetType().Name} received.") |> raise
        | HttpErrorRequestResponse(status, body, headers, errors) -> error (status, body, headers, errors)
        | HttpExceptionRequestResponse ex -> raise ex
        | HttpOkRequestResponse(status, body, contentType, headers) -> ok (status, body, contentType, headers)

    let private handleOkResponse ok =
        handleResponse ok (fun (status, _, _, _) -> new Exception($"{status} received.") |> raise)

    let getHeartbeat host =
        task {
            let uri = Http.route host "api/v1/heartbeat/"

            let req = new HttpRequestMessage(HttpMethod.Get, uri)

            let! resp = send req

            return
                match resp with
                | HttpOkRequestResponse _ -> true
                | _ -> false
        }



    let getStats host =
        task {
            let uri = Http.route host "api/v1/stats/"

            let req = new HttpRequestMessage(HttpMethod.Get, uri)

            let! resp = send req

            return
                resp
                |> handleOkResponse (fun (_, body, _, _) ->
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Discorss.ApiModels.Stats[]> body)
        }


    let getFeeds host =
        task {
            let uri = Http.route host "api/v1/feeds/"

            let req = new HttpRequestMessage(HttpMethod.Get, uri)

            let! resp = send req

            return
                resp
                |> handleOkResponse (fun (_, body, _, _) ->
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Discorss.ApiModels.FeedInfo[]> body)
        }

    let previewFeeds host feedUri =
        task {
            let uri = Http.route host $"api/v1/feeds/{Http.encode feedUri}/"

            let req = new HttpRequestMessage(HttpMethod.Get, uri)

            let! resp = send req

            return
                resp
                |> handleOkResponse (fun (_, body, _, _) ->
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
                |> handleOkResponse (fun (_, body, _, _) ->
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Discorss.ApiModels.FeedInfo> body)
        }

    let deleteFeed host feedUri =
        task {
            let uri = Http.route host $"api/v1/feeds/{Http.encode feedUri}/"

            let req = new HttpRequestMessage(HttpMethod.Delete, uri)

            let! resp = send req

            return
                resp
                |> handleOkResponse (fun (_, body, _, _) -> ignore body)
        }

    let nextDocument host =
        task {
            let uri = Http.route host "api/v1/documents/queue/"

            let req = new HttpRequestMessage(HttpMethod.Get, uri)

            let! resp = send req

            return
                resp
                |> handleOkResponse (fun (status, body, _, _) ->
                    match status with
                    | HttpStatusCode.OK ->
                        body
                        |> Newtonsoft.Json.JsonConvert.DeserializeObject<ApiModels.Document>
                        |> Some
                    | _ -> None)
        }

    let getLikeDocument host (document: Document) =
        task {
            let uri =
                document.uri
                |> Http.encode
                |> sprintf "api/v1/documents/likes/%s/"
                |> Http.route host

            let req = new HttpRequestMessage(HttpMethod.Get, uri)

            let! resp = send req

            return
                resp
                |> handleResponse
                    (fun (_, body, _, _) ->
                        Newtonsoft.Json.JsonConvert.DeserializeObject<Discorss.ApiModels.DocumentLike> body
                        |> Some)
                    (fun (status, body, headers, errors) -> None)
        }

    let likeDocument host like (document: Document) =
        task {
            let uri = Http.route host "api/v1/documents/likes/"

            let req =
                { ApiModels.DocumentLike.uri = document.uri
                  liked = like }

            let body = req |> Newtonsoft.Json.JsonConvert.SerializeObject
            let req = new HttpRequestMessage(HttpMethod.Put, uri) |> Http.applyJsonContent body

            let! resp = send req

            return
                resp
                |> handleOkResponse (fun (_, body, _, _) ->
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Discorss.ApiModels.DocumentLike> body)
        }

    let deleteDocumentLike host (document: Document) =
        task {
            let uri =
                document.uri
                |> Http.encode
                |> sprintf "api/v1/documents/likes/%s/"
                |> Http.route host

            let req = new HttpRequestMessage(HttpMethod.Delete, uri)

            let! resp = send req

            return resp |> handleOkResponse (fun (_, body, _, _) -> ignore body)
        }
