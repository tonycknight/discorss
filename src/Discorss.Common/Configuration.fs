namespace Discorss

module ApiPorts =

    [<Literal>]
    let feedServicePort = 63530

    [<Literal>]
    let indexServicePort = 63531

    [<Literal>]
    let ingestionServicePort = 63532

    [<Literal>]
    let msgHubServicePort = 63533

[<CLIMutable>]
type AppConfiguration =
    { indexServiceUrl: string
      feedServiceUrl: string
      ingestionServiceUrl: string
      messageHubServiceUrl: string
      secretsConnection: string }

    static member defaultConfig =
        { AppConfiguration.indexServiceUrl = $"http://localhost:{ApiPorts.indexServicePort}"
          feedServiceUrl = $"http://localhost:{ApiPorts.feedServicePort}"
          ingestionServiceUrl = $"http://localhost:{ApiPorts.ingestionServicePort}"
          messageHubServiceUrl = $"http://localhost:{ApiPorts.msgHubServicePort}"
          secretsConnection = "" }

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
