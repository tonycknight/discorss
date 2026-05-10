namespace Discorss

open System.Threading.Tasks

type ActorStats =
    { name: string
      queueCount: int64
      childStats: ActorStats list }

type IStatsSource =
    abstract member GetStatsAsync: unit -> Task<ActorStats>