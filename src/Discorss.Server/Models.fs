namespace Discorss.Server

open Discorss.ApiModels

module Models =
    let rec toStats (value: Discorss.ActorStats) =
        { Stats.name = value.name
          itemCount = value.queueCount
          childStats = value.childStats |> List.map toStats }
