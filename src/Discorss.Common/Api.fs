namespace Discorss

open Discorss
open Discorss.ApiModels
open Giraffe
open Microbroker.Client
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

module Api =

    let heartbeatRoute: HttpHandler =
        route "/heartbeat" >=> noResponseCaching >=> json [ "OK" ]

    let config (sp: System.IServiceProvider) =
        let c = AppConfiguration.defaultConfig

        sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>().GetSection("Discorss").Bind(c)

        c

    let errorHandler: ErrorHandler =
        fun (ex: exn) (logger: ILogger) ->
            [ ex.GetType().FullName; ex.Message; ex.StackTrace ]
            |> Strings.join System.Environment.NewLine
            |> logger.LogError

            clearResponse
            >=> publicResponseCaching 5 None
            >=> ServerErrors.internalError (json ({ ApiErrorResult.errors = [| "An unhandled error occurred." |] }))

    let logClient: HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let logger = ctx.GetService<ILoggerFactory>().CreateLogger()
            logger.LogInformation($"Request remote IP: {ctx.Connection.RemoteIpAddress}:{ctx.Connection.RemotePort}")
            next ctx


module ApiStartup =

    let addApiLogging (services: IServiceCollection) =
        services
            .AddLogging()
            .AddHttpLogging(fun lo ->
                lo.LoggingFields <- Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders)

    let addApiConfig (services: IServiceCollection) =
        services
            .AddSingleton<AppConfiguration>(fun sp -> Api.config sp)
            .AddSingleton<IExternalHttpClient, ExternalHttpClient>()
            .AddSingleton<Discorss.Security.ISecretProvider>(new Discorss.Security.StubSecretProvider())

    let addApiHttp (services: IServiceCollection) =
        services
            .AddHttpClient()
            .AddSingleton<IInternalHttpClient, InternalHttpClient>()
            .AddSingleton<IExternalHttpClient, ExternalHttpClient>()
            
    let addMicrobroker (services: IServiceCollection) = 
        let config (sp: System.IServiceProvider) =
            let appConfig = sp.GetRequiredService<AppConfiguration>()
            let throttle = System.TimeSpan.FromSeconds 5.

            { MicrobrokerConfiguration.brokerBaseUrl = appConfig.messageBrokerServiceUrl
              throttleMaxTime = throttle }
        
        DependencyInjection.addServices services
        |> DependencyInjection.addConfiguration config 
        
    let addWebFramework (services: IServiceCollection) = services.AddGiraffe()

    let addApi<'a when 'a :> IServiceCollection> =
        addApiLogging >> addApiConfig >> addApiHttp >> addWebFramework >> addMicrobroker

    let configureAppConfig (whbc: IConfigurationBuilder) =
        whbc.AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables("Discorss_")
        |> ignore

module ApiValidation =
    let getRequest<'a> (ctx: HttpContext) =
        task {
            if
                ctx.Request.ContentType <> System.Net.Mime.MediaTypeNames.Application.Json
                && ctx.Request.ContentType
                   <> $"{System.Net.Mime.MediaTypeNames.Application.Json}; charset=utf-8"
            then
                let result = { ApiErrorResult.errors = [| "Invalid content type" |] }
                return Choice1Of2 result
            else
                try
                    let! msg = ctx.BindModelAsync<'a>()

                    return
                        match System.Object.ReferenceEquals(msg, null) with
                        | false -> Choice2Of2 msg
                        | true -> Choice1Of2 { ApiErrorResult.errors = [| "Invalid request" |] }
                with ex ->
                    return Choice1Of2 { ApiErrorResult.errors = [| "Invalid request" |] }
        }
