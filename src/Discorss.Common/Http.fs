namespace Discorss.Common

open System
open System.Net
open System.Net.Http

type HttpRequestResponse =
    | HttpOkRequestResponse of status: HttpStatusCode * body: string
    | HttpRetryableRequestResponse of status: HttpStatusCode * body: string
    | HttpErrorRequestResponse of status: HttpStatusCode * body: string
    | HttpExceptionRequestResponse of ex: Exception

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