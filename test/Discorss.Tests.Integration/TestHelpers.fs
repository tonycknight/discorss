namespace Discorss.Tests.Integration

open System
open Discorss
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open NSubstitute

module TestHelpers =
    let config () =
        { AppConfiguration.defaultConfig with
            mongoDbName = "discorss_inttests" }

    let configOptions (config: AppConfiguration) =
        let result = Substitute.For<IOptions<AppConfiguration>>()
        result.Value.Returns config |> ignore
        result

    let logFactory () = Substitute.For<ILoggerFactory>()
