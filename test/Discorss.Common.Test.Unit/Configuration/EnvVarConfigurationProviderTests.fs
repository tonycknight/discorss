namespace Discorss.Feeds.Test.Unit.Configuration

open System
open Discorss
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit
open Discorss.Configuration

module EnvVarConfigurationProviderTests=

    [<Xunit.Theory>]
    [<Xunit.InlineData("")>]
    [<Xunit.InlineData(null)>]
    [<Xunit.InlineData("  ")>]
    let ``GetConfig yields default when envvars are null``(value)=

        let ev _ = value

        let evp = new EnvVarConfigurationProvider(ev) :> IConfigurationProvider

        let c = evp.GetConfig()
        let dc = AppConfiguration.defaultConfig()
        
        c.ingestionServiceUrl |> should equal dc.ingestionServiceUrl
        c.messageHubServiceUrl |> should equal dc.messageHubServiceUrl
        c.feedServiceUrl |> should equal dc.feedServiceUrl
        c.indexServiceUrl |> should equal dc.indexServiceUrl
        c.secretsConnection |> should equal dc.secretsConnection

    // TODO: not happy with this test, it has too many assumptions
    [<Property(Verbose = true)>]
    let ``GetConfig yields envvar value``(NonEmptyString value) =
        let ev (n:string) =     let n = n.Substring("Discorss_".Length)
                                $"{n}_{value}"

        let evp = new EnvVarConfigurationProvider(ev) :> IConfigurationProvider

        let c = evp.GetConfig()
        
        // TODO: really need Asserts in a property test?
        c.ingestionServiceUrl |> should equal $"ingestionServiceUrl_{value}"
        c.messageHubServiceUrl |> should equal $"messageHubServiceUrl_{value}"
        c.feedServiceUrl |> should equal $"feedServiceUrl_{value}"
        c.indexServiceUrl |> should equal $"indexServiceUrl_{value}"
        c.secretsConnection |> should equal $"secretsConnection_{value}"
