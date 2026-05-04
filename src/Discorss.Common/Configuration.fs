namespace Discorss

open System

[<CLIMutable>]
type AppConfiguration =
    { microbrokerServiceUrl: string
      microbrokerThrottle: TimeSpan
      documentIngestionWindow: TimeSpan
      queuePollFrequency: TimeSpan
      feedIngestionFrequency: TimeSpan
      mongoConnection: string
      mongoDbName: string }

    static member sectionName = "Discorss"

    static member defaultConfig =
        { AppConfiguration.microbrokerServiceUrl = "http://localhost:8080"
          microbrokerThrottle = TimeSpan.FromSeconds 2.
          documentIngestionWindow = TimeSpan.FromMinutes 60.
          queuePollFrequency = TimeSpan.FromSeconds 5.
          feedIngestionFrequency = TimeSpan.FromSeconds 15.
          mongoConnection = "mongodb://localhost:27017"
          mongoDbName = "discorss" }
