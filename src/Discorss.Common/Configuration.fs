namespace Discorss

open System

module ApiPorts =

    [<Literal>]
    let servicePort = 8081

[<CLIMutable>]
type AppConfiguration =
    { microbrokerServiceUrl: string
      microbrokerThrottle: TimeSpan
      secretsConnection: string
      documentIngestionWindow: TimeSpan 
      queuePollFrequency: TimeSpan 
      feedIngestionFrequency: TimeSpan }

    static member defaultConfig =
        { AppConfiguration.microbrokerServiceUrl = "http://localhost:8080"
          microbrokerThrottle = TimeSpan.FromSeconds 2.
          secretsConnection = "" 
          documentIngestionWindow = TimeSpan.FromMinutes 60.
          queuePollFrequency = TimeSpan.FromSeconds 5. 
          feedIngestionFrequency = TimeSpan.FromSeconds 15. } // TODO: 

module Configuration =
    open System.Reflection

    let private configProp =
        let props =
            typeof<AppConfiguration>.GetProperties()
            |> Seq.map (fun pi -> (pi.Name, pi))
            |> Map.ofSeq

        fun n -> props |> Map.find n

    let private propValue c (pi: PropertyInfo) =
        pi.GetGetMethod().Invoke(c, [||]) :?> string

    let private configCtor =
        typeof<AppConfiguration>.GetConstructors()
        |> Seq.sortByDescending (fun c -> c.GetParameters().Length)
        |> Seq.head

    let private ctorParams = configCtor.GetParameters()

    let mergeDefaults config =
        let propValue c = configProp >> propValue c

        let paramValues =
            ctorParams
            |> Array.map (fun p ->
                let v =
                    let cv = p.Name |> propValue config

                    if cv |> System.String.IsNullOrWhiteSpace |> not then
                        cv
                    else
                        p.Name |> propValue AppConfiguration.defaultConfig

                v :> obj)

        configCtor.Invoke(paramValues) :?> AppConfiguration
