namespace Discorss

open System.Net
open Discorss
open Discorss.Security
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

[<CLIMutable>]
type ApiErrorResult = {
    errors: string[]
    }
    

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

    let heartbeatRoute : HttpHandler = route "/heartbeat"      >=> json [ "OK" ]

    let isAuthorised (sp: System.IServiceProvider): HttpHandler =         
        let secrets = sp.GetRequiredService<ISecretProvider>()
        requiresValidIp >=> requiresApiKey secrets


module ApiStartup =

    let addApiLogging(services: IServiceCollection)=
        services.AddLogging()
                .AddHttpLogging(fun lo -> lo.LoggingFields <- Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.Request)

    let addApiConfig(services: IServiceCollection)=
        services.AddSingleton<Discorss.Configuration.IConfigurationProvider, Discorss.Configuration.ConfigurationProvider>()                
                .AddSingleton<Discorss.IExternalHttpClient, Discorss.ExternalHttpClient>()
                .AddSingleton<Discorss.Security.ISecretProvider, Discorss.Security.StubSecretProvider>()
        
    let addApiHttp(services: IServiceCollection)=
        services.AddHttpClient()
                .AddSingleton<Discorss.IInternalHttpClient, Discorss.InternalHttpClient>()
                .AddSingleton<Discorss.IExternalHttpClient, Discorss.ExternalHttpClient>()
                .AddSingleton<Discorss.Messaging.IMessageHubClient, Discorss.Messaging.MessageHubClient>()
    
    let addWebFramework(services: IServiceCollection)=
        services.AddGiraffe() 

    let addApi<'a when 'a :> IServiceCollection> = addApiLogging >> addApiConfig >> addApiHttp >> addWebFramework

module ApiValidation =
    let getRequest<'a>(ctx: HttpContext)=   
        task {
            if ctx.Request.ContentType <> "application/json" then
                let result = { ApiErrorResult.errors = [| "Invalid content type" |] }
                return Choice1Of2 result
            else
                let! msg = ctx.BindModelAsync<'a>()
                return match System.Object.ReferenceEquals(msg, null) with                        
                        | false -> Choice2Of2 msg
                        | true -> Choice1Of2 { ApiErrorResult.errors = [| "Invalid request" |] }
            }