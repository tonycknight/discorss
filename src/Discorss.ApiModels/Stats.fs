namespace Discorss.ApiModels

open System

[<CLIMutable>]
type Stats =
    { name: string
      itemCount: int64
      childStats: Stats list }
