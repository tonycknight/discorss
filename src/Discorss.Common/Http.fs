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
    
    let parse (resp: HttpResponseMessage) =
        match resp.IsSuccessStatusCode with
        | true -> task {
                    let! body = resp.Content.ReadAsStringAsync()
                    return HttpOkRequestResponse(resp.StatusCode, body)
                    }
        | false -> task {
                    let! body = resp.Content.ReadAsStringAsync()
                    return HttpErrorRequestResponse(resp.StatusCode, body)
                    }

    let get (client: HttpClient) (msg: HttpRequestMessage) =
        task {
            try
                let! resp = client.SendAsync msg
                return! parse resp        
            with
            | ex ->
                return HttpExceptionRequestResponse(ex)            
        }

type IInternalHttpClient=
    abstract member GetAsync : url:string -> Task<HttpRequestResponse>

[<ExcludeFromCodeCoverage>]
type InternalHttpClient(httpClient: HttpClient, secrets: Security.ISecretProvider)=
    let httpGet = Http.get httpClient
    
    let req (url: string) = 
        let result = new HttpRequestMessage(HttpMethod.Get, url)
        result.Headers.Add("x-api-key", secrets.GetSecretValue "apikey")
        result

    interface IInternalHttpClient with
        member this.GetAsync(url) = url |> req |> httpGet


type IExternalHttpClient=
    abstract member GetAsync : url:string -> Task<HttpRequestResponse>
  
[<ExcludeFromCodeCoverage>]
type ExternalHttpClient(httpClient: HttpClient)=
    let httpGet = Http.get httpClient
    
    // TODO: log req/resp?
    let req (url:string) = new HttpRequestMessage(HttpMethod.Get, url)

    interface IExternalHttpClient with
        member this.GetAsync(url) = url |> req |> httpGet

