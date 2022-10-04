namespace Discorss.Security

open System.Net
open Discorss
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

module Api =    
    let private isHomeIp (ctx: HttpContext) = ctx.Connection.RemoteIpAddress |> IPAddress.IsLoopback
                    
    let private isValidApiKey (secrets: ISecretProvider) (ctx: HttpContext) = 
        match ctx.TryGetRequestHeader "x-api-key" with
        | Some k -> secrets.IsSecretValueEqual "apikey" k
        | None -> false 

    let private accessDenied  : HttpHandler = setStatusCode 401 >=> setBody [||]
    let private forbidden  : HttpHandler = setStatusCode 403 >=> setBody [||]

    let private requiresValidIp : HttpHandler = authorizeRequest isHomeIp forbidden
    let private requiresApiKey secrets: HttpHandler = authorizeRequest (isValidApiKey secrets >||> isHomeIp) accessDenied

    let isAuthorised (sp: System.IServiceProvider): HttpHandler =         
        let secrets = sp.GetRequiredService<ISecretProvider>()
        requiresValidIp >=> requiresApiKey secrets
