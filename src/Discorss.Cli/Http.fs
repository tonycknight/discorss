namespace Discorss

open System
open System.Net
open System.Net.Http
open System.Threading
open Newtonsoft.Json.Linq

type HttpResponseHeaders = (string * string) list

[<CLIMutable>]
type HttpResponseErrors =
    { errors: string[] }

    static member empty = { errors = [||] }

type HttpRequestResponse =
    | HttpOkRequestResponse of
        status: HttpStatusCode *
        body: string *
        contentType: string option *
        headers: HttpResponseHeaders
    | HttpTooManyRequestsResponse of headers: HttpResponseHeaders
    | HttpBadGatewayResponse of headers: HttpResponseHeaders
    | HttpErrorRequestResponse of
        status: HttpStatusCode *
        body: string *
        headers: HttpResponseHeaders *
        errors: HttpResponseErrors
    | HttpExceptionRequestResponse of ex: Exception

    static member status(response: HttpRequestResponse) =
        match response with
        | HttpOkRequestResponse(status, _, _, _) -> status
        | HttpTooManyRequestsResponse(_) -> HttpStatusCode.TooManyRequests
        | HttpErrorRequestResponse(status, _, _, _) -> status
        | HttpExceptionRequestResponse _ -> HttpStatusCode.InternalServerError
        | HttpBadGatewayResponse _ -> HttpStatusCode.BadGateway

    static member loggable(response: HttpRequestResponse) =
        let status = HttpRequestResponse.status response
        $"{response.GetType().Name} {status}"

module Http =
    let private body (cancellation: CancellationToken) (resp: HttpResponseMessage) =
        task {
            let! body =
                match resp.Content.Headers.ContentEncoding |> Seq.tryHead with
                | Some x when x = "gzip" ->
                    task {
                        use s = resp.Content.ReadAsStream(cancellation)
                        return Strings.fromGzip s
                    }
                | _ -> resp.Content.ReadAsStringAsync()

            return body
        }

    let private errors body =
        match body with
        | "" -> HttpResponseErrors.empty
        | json ->
            let jq = JObject.Parse json

            let msgs =
                jq.SelectTokens("errors").Values()
                |> Seq.map (fun t -> t.ToString())
                |> Array.ofSeq

            { HttpResponseErrors.empty with
                errors = msgs }

    let private contentHeaders (resp: HttpResponseMessage) =
        resp.Content.Headers
        |> Seq.collect (fun x -> x.Value |> Seq.map (fun v -> Strings.toLower x.Key, v))

    let private respHeaders (resp: HttpResponseMessage) =
        resp.Headers
        |> Seq.collect (fun x -> x.Value |> Seq.map (fun v -> (Strings.toLower x.Key, v)))

    let private headers (resp: HttpResponseMessage) =
        respHeaders resp
        |> Seq.append (contentHeaders resp)
        |> Seq.sortBy fst
        |> List.ofSeq

    let private parse (cancellation: CancellationToken) (resp: HttpResponseMessage) =
        let respHeaders = headers resp

        match resp.IsSuccessStatusCode, resp.StatusCode with
        | true, _ ->
            task {
                let! body = body cancellation resp

                let mediaType =
                    resp.Content.Headers.ContentType
                    |> Option.ofNull<Headers.MediaTypeHeaderValue>
                    |> Option.map _.MediaType

                return HttpOkRequestResponse(resp.StatusCode, body, mediaType, respHeaders)
            }
        | false, HttpStatusCode.TooManyRequests -> HttpTooManyRequestsResponse(respHeaders) |> Tasks.toTaskResult
        | false, HttpStatusCode.BadGateway -> HttpBadGatewayResponse(respHeaders) |> Tasks.toTaskResult
        | false, HttpStatusCode.BadRequest ->
            task {
                let! body = body cancellation resp

                return HttpErrorRequestResponse(resp.StatusCode, body, respHeaders, errors body)
            }
        | false, _ ->
            task {
                let! body = body cancellation resp
                return HttpErrorRequestResponse(resp.StatusCode, body, respHeaders, HttpResponseErrors.empty)
            }

    let applyJsonContent (content: string) (request: HttpRequestMessage) =
        request.Content <-
            new System.Net.Http.StringContent(
                content,
                Text.Encoding.UTF8,
                System.Net.Mime.MediaTypeNames.Application.Json
            )

        request

    let route (host: string) path =
        let ub = new UriBuilder(host)
        ub.Path <- path
        ub.Uri

    let encode (uri: string) = System.Web.HttpUtility.UrlEncode uri

    let client =
        let client = new HttpClient()
        (fun () -> client)

    let send (cancellation: CancellationToken) (client: HttpClient) (msg: HttpRequestMessage) =
        task {
            try
                use! resp = client.SendAsync(msg, cancellation)
                return! parse cancellation resp
            with ex ->
                return HttpExceptionRequestResponse(ex)
        }
