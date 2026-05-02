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
                microbrokerServiceUrl = value }

        let dc = AppConfiguration.defaultConfig

        let c2 = Configuration.mergeDefaults c

        c2.microbrokerServiceUrl |> should equal dc.microbrokerServiceUrl
        c2.secretsConnection |> should equal dc.secretsConnection


    [<Property(Verbose = true)>]
    let ``mergeDefaults yields new value`` (value: NonWhiteSpaceString) =
        let c =
            { AppConfiguration.defaultConfig with
                microbrokerServiceUrl = value.Get }

        let dc = AppConfiguration.defaultConfig

        let c2 = Configuration.mergeDefaults c

        c2.microbrokerServiceUrl |> should equal dc.microbrokerServiceUrl
        c2.secretsConnection |> should equal dc.secretsConnection
