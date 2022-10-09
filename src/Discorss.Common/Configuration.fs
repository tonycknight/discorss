namespace Discorss.Configuration

module ApiPorts=
    
    [<Literal>]
    let feedServicePort = 63530

    [<Literal>]
    let indexServicePort = 63531

    [<Literal>]
    let ingestionServicePort = 63532

    [<Literal>]
    let hubServicePort = 63533

[<CLIMutable>] // TODO:!!!
type AppConfiguration = {
        indexServiceUrl :       string
        feedServiceUrl :        string
        ingestionServiceUrl :   string
        messageHubServiceUrl :  string
        secretsConnection:      string
    }
    with static member defaultConfig() = 
            { AppConfiguration.indexServiceUrl = $"http://localhost:{ApiPorts.indexServicePort}";
                                feedServiceUrl = $"http://localhost:{ApiPorts.feedServicePort}";
                                ingestionServiceUrl = $"http://localhost:{ApiPorts.ingestionServicePort}";
                                messageHubServiceUrl = $"http://localhost:{ApiPorts.hubServicePort}";
                                secretsConnection = ""
                                }

type IConfigurationProvider =
    abstract member GetConfig : unit -> AppConfiguration

type ConfigurationProvider() =
    interface IConfigurationProvider with
        member this.GetConfig()= AppConfiguration.defaultConfig()

type EnvVarConfigurationProvider(getEnvVar: string -> string) =
    
    let configProps = typeof<AppConfiguration>.GetProperties()
    let configPropNames = configProps |> Seq.map (fun pi -> pi.Name) 
    let configProp n = configProps |> Seq.find (fun pi -> pi.Name = n)

    // TODO: clean up
    let configCtors = typeof<AppConfiguration>.GetConstructors() |> Seq.sortByDescending (fun c -> c.GetParameters().Length)
    let configCtor = configCtors |> Seq.head
        

    let envVars()=
        configPropNames
            |> Seq.map (fun k -> (k, getEnvVar($"Discorss_{k}") )) 
            |> Map.ofSeq

    let createFromEnvVars config envVars =
        let ctorParams = configCtor.GetParameters()
        
    
        let paramValues = ctorParams |> Seq.map (fun p ->   let v = match envVars |> Map.tryFind p.Name with
                                                                    | Some ev when (System.String.IsNullOrWhiteSpace(ev) |> not) -> 
                                                                            ev 
                                                                    | _ ->  // TODO: get from original config
                                                                            let pi = configProp p.Name
                                                                            pi.GetGetMethod().Invoke(config, [| |]) :?> string
                                                            (p, v))
                                      |> Seq.map (fun t ->  let v = snd t
                                                            v :> obj)
                                      |> Array.ofSeq
        configCtor.Invoke(paramValues) :?> AppConfiguration


    // TODO: properties have no Set method
    let applyEnvVars config envVars=        
        configProps
                |> Seq.iter (fun pi ->  match envVars |> Map.tryFind pi.Name with
                                        | Some v when v <> null -> 
                                                    pi.GetSetMethod().Invoke(config, [| v |] ) |> ignore 
                                        | _ ->      ignore 0
                                )
        config
        
    //let config = envVars() |> applyEnvVars (AppConfiguration.defaultConfig())
    let config = envVars() |> createFromEnvVars (AppConfiguration.defaultConfig())
    
    new() = EnvVarConfigurationProvider(System.Environment.GetEnvironmentVariable)
    
    interface IConfigurationProvider with
        member this.GetConfig()= config
