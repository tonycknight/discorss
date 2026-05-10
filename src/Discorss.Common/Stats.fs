namespace Discorss

open System.Threading.Tasks

type Stats =
    { name: string
      itemCount: int64
      childStats: Stats list }

type IStatsSource =
    abstract member GetStatsAsync: unit -> Task<Stats>
