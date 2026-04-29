namespace Discorss.Feeds.Test.Unit

open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit
open Discorss

module ConfigurationTests =

    [<Xunit.Theory>]
    [<Xunit.InlineData("")>]
    [<Xunit.InlineData(null)>]
    [<Xunit.InlineData("  ")>]
    let ``merge yields default when envvars are null`` (value) =

        let c =
            { AppConfiguration.defaultConfig with
                messageHubServiceUrl = value }

        let dc = AppConfiguration.defaultConfig

        let c2 = Configuration.mergeDefaults c

        c2.messageHubServiceUrl |> should equal dc.messageHubServiceUrl
        c2.secretsConnection |> should equal dc.secretsConnection


    [<Property(Verbose = true)>]
    let ``mergeDefaults yields new value`` (value: NonWhiteSpaceString) =
        let c =
            { AppConfiguration.defaultConfig with
                messageHubServiceUrl = value.Get }

        let dc = AppConfiguration.defaultConfig

        let c2 = Configuration.mergeDefaults c

        c2.messageHubServiceUrl |> should equal dc.messageHubServiceUrl
        c2.secretsConnection |> should equal dc.secretsConnection
