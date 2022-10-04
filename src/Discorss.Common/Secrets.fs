namespace Discorss.Security

open System
open System.Diagnostics.CodeAnalysis

type ISecretProvider=
    abstract member IsSecretValueEqual : string -> string -> bool
    abstract member GetSecretValue : string -> string 

[<ExcludeFromCodeCoverage>]
type StubSecretProvider(secrets: (string * string) seq)=
    let secrets = secrets |> Map.ofSeq
    
    new() = StubSecretProvider([ ("apikey", "abc"); ])

    interface ISecretProvider with
        member this.IsSecretValueEqual name value =
            match secrets |> Map.tryFind name with
                | Some v -> StringComparer.Ordinal.Equals(v, value)
                | _ -> false
            
        member this.GetSecretValue name =
            match secrets |> Map.tryFind name with
                | Some v -> v
                | _ -> ""