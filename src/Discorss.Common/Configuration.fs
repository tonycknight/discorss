namespace Discorss.Configuration

type AppConfiguration = {
        indexServiceUrl :       string
        feedServiceUrl :        string
        ingestionServiceUrl :   string
    }

type IConfigurationProvider =
    abstract member GetConfig : unit -> AppConfiguration

type ConfigurationProvider() =
    interface IConfigurationProvider with
        member this.GetConfig()=
            { AppConfiguration.indexServiceUrl = "http://localhost:63532";
                                feedServiceUrl = "http://localhost:62369";
                                ingestionServiceUrl = "http://localhost:63530"}