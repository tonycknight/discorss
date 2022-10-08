namespace Discorss.Configuration

type AppConfiguration = {
        indexServiceUrl :       string
        feedServiceUrl :        string
        ingestionServiceUrl :   string
        messageHubServiceUrl :  string
    }

module ApiPorts=
    
    [<Literal>]
    let feedServicePort = 63530

    [<Literal>]
    let indexServicePort = 63531

    [<Literal>]
    let ingestionServicePort = 63532

    [<Literal>]
    let hubServicePort = 63533

type IConfigurationProvider =
    abstract member GetConfig : unit -> AppConfiguration

type ConfigurationProvider() =
    interface IConfigurationProvider with
        member this.GetConfig()=
            { AppConfiguration.indexServiceUrl = $"http://localhost:{ApiPorts.indexServicePort}";
                                feedServiceUrl = $"http://localhost:{ApiPorts.feedServicePort}";
                                ingestionServiceUrl = $"http://localhost:{ApiPorts.ingestionServicePort}";
                                messageHubServiceUrl = $"http://localhost:{ApiPorts.hubServicePort}"
                                }