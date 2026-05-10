namespace Discorss.Server

open Discorss.ApiModels

module Models =
    let rec toStats (value: Discorss.Stats) =
        { Stats.name = value.name
          itemCount = value.itemCount
          childStats = value.childStats |> List.map toStats }
