namespace Discorss

open System
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks
open System.Net
open System.Net.Http

type HttpRequestResponse =
    | HttpOkRequestResponse of status: HttpStatusCode * body: string
    | HttpErrorRequestResponse of status: HttpStatusCode * body: string
    | HttpExceptionRequestResponse of ex: Exception

[<ExcludeFromCodeCoverage>]
module Uri =
    let tryParse (uri: string) =
        match Uri.IsWellFormedUriString(uri, UriKind.Absolute) with
        | true -> new Uri(uri) |> Some
        | _ -> None

[<ExcludeFromCodeCoverage>]
module Http =
    let private body (resp: HttpResponseMessage) =
        task {
            let! body =
                match resp.Content.Headers.ContentEncoding |> Seq.tryHead with
                | Some x when x = "gzip" ->
                    task {
                        use s = resp.Content.ReadAsStream(System.Threading.CancellationToken.None)
                        return String.fromGzip s
                    }
                | _ -> resp.Content.ReadAsStringAsync()

            return body
        }

    let parse (resp: HttpResponseMessage) =
        match resp.IsSuccessStatusCode with
        | true ->
            task {
                let! body = body resp
                return HttpOkRequestResponse(resp.StatusCode, body)
            }
        | false ->
            task {
                let! body = body resp
                return HttpErrorRequestResponse(resp.StatusCode, body)
            }

    let send (client: HttpClient) (msg: HttpRequestMessage) =
        task {
            try
                let! resp = client.SendAsync msg
                return! parse resp
            with ex ->
                return HttpExceptionRequestResponse(ex)
        }

type IExternalHttpClient =
    abstract member GetAsync: url: string -> Task<HttpRequestResponse>

[<ExcludeFromCodeCoverage>]
type ExternalHttpClient(httpClient: HttpClient) =
    let httpSend = Http.send httpClient

    let req (url: string) =
        let result = new HttpRequestMessage(HttpMethod.Get, url)
        result.Headers.Add("User-Agent", "discorss")
        result.Headers.Add("Accept-Encoding", "gzip")
        result

    interface IExternalHttpClient with
        member this.GetAsync(url) = url |> req |> httpSend
