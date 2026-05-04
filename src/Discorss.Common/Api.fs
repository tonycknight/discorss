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

    [<Literal>]
    let servicePort = 8081

    let validContentTypes =
        let json = System.Net.Mime.MediaTypeNames.Application.Json
        [ json; $"{json}; charset=utf-8" ]

    let isValidContentType (ctx: HttpContext) =
        validContentTypes |> Seq.contains ctx.Request.ContentType

    let heartbeatRoute: HttpHandler =
        route "/heartbeat" >=> noResponseCaching >=> json [ "OK" ]

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

    let getRequest<'a> (ctx: HttpContext) =
        task {            
            if isValidContentType ctx |> not then
                return Choice1Of2 "Invalid content type"
            else
                try
                    let! msg = ctx.BindModelAsync<'a>()
                    return
                        match System.Object.ReferenceEquals(msg, null) with
                        | false -> Choice2Of2 msg
                        | true -> Choice1Of2 "Invalid request"
                with ex ->
                    return Choice1Of2 "Invalid request"
        }

module ApiStartup =

    let addApiLogging (services: IServiceCollection) =
        services
            .AddLogging()
            .AddHttpLogging(fun lo ->
                lo.LoggingFields <- Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders)

    let addApiConfig (services: IServiceCollection) =
        services.AddOptions<AppConfiguration>().BindConfiguration(AppConfiguration.sectionName).ValidateOnStart()
        |> ignore

        services

    let addApiHttp (services: IServiceCollection) =
        services.AddHttpClient().AddSingleton<IExternalHttpClient, ExternalHttpClient>()

    let addMicrobroker (services: IServiceCollection) =
        let config (sp: System.IServiceProvider) =
            let appConfig =
                sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppConfiguration>>()

            { MicrobrokerConfiguration.brokerBaseUrl = appConfig.Value.microbrokerServiceUrl
              throttleMaxTime = appConfig.Value.microbrokerThrottle }

        DependencyInjection.addServices services
        |> DependencyInjection.addConfiguration config

    let addWebFramework (services: IServiceCollection) = services.AddGiraffe()

    let addCaching (services: IServiceCollection) = services.AddMemoryCache()

    let addApi<'a when 'a :> IServiceCollection> =
        addApiLogging
        >> addApiConfig
        >> addApiHttp
        >> addWebFramework
        >> addMicrobroker
        >> addCaching

    let configureAppConfig (args: string[]) (whbc: IConfigurationBuilder) =
        whbc.AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables("Discorss_").AddCommandLine args
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
