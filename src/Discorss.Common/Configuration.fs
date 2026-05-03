namespace Discorss

open System

[<CLIMutable>]
type AppConfiguration =
    { microbrokerServiceUrl: string
      microbrokerThrottle: TimeSpan
      secretsConnection: string
      documentIngestionWindow: TimeSpan 
      queuePollFrequency: TimeSpan 
      feedIngestionFrequency: TimeSpan }

    static member sectionName = "Discorss"

    static member defaultConfig =
        { AppConfiguration.microbrokerServiceUrl = "http://localhost:8080"
          microbrokerThrottle = TimeSpan.FromSeconds 2.
          secretsConnection = "" 
          documentIngestionWindow = TimeSpan.FromMinutes 60.
          queuePollFrequency = TimeSpan.FromSeconds 5. 
          feedIngestionFrequency = TimeSpan.FromSeconds 15. }
