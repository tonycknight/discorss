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

    let get (client: HttpClient) (url: string) =
        task {
            try
                let! resp = client.GetAsync(url)
                return! parse resp        
            with
            | ex ->
                return HttpExceptionRequestResponse(ex)            
        }

type IExternalHttpClient=
    abstract member GetAsync : url:string -> Task<HttpRequestResponse>
   

type IExternalHttpClientFactory=
    abstract member GetHttpClient: name:string -> IExternalHttpClient

[<ExcludeFromCodeCoverage>]
type ExternalHttpClient(httpClient: HttpClient)=
    let httpGet = Http.get httpClient

    interface IExternalHttpClient with
        member this.GetAsync(url) = httpGet url

[<ExcludeFromCodeCoverage>]
type ExternalHttpClientFactory(client: IExternalHttpClient)=
    
    interface IExternalHttpClientFactory with
        member this.GetHttpClient(name) = client
