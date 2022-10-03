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
    let httpClientFactory () =        
        // TODO: 
        let hc = new HttpClient()
        // hc.DefaultRequestHeaders.UserAgent <- "discorss"        
        hc
    
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
    abstract member get : url:string -> Task<HttpRequestResponse>
   

type IExternalHttpClientFactory=
    abstract member httpClient: name:string -> IExternalHttpClient

type ExternalHttpClient(httpClient: HttpClient)=
    let httpGet = Http.get httpClient

    interface IExternalHttpClient with
        member this.get(url) = httpGet url


type ExternalHttpClientFactory()=
    let client = Http.httpClientFactory()
    interface IExternalHttpClientFactory with
        member this.httpClient(name) = new ExternalHttpClient(client)
