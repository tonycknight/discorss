namespace Discorss.Documents

open System
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open MongoDB.Bson

type IWordStatisticsRepository =
    abstract member AddAsync: DocumentStatistics -> Task

type StubWordStatisticsRepository() =

    let cache =
        new System.Collections.Concurrent.ConcurrentDictionary<string, WordStatistics>(
            StringComparer.InvariantCultureIgnoreCase
        )

    interface IWordStatisticsRepository with
        member this.AddAsync(stats: DocumentStatistics) =
            task {
                // TODO:
                return 0
            }

type IDocumentStatisticsRepository =
    abstract member SetAsync: DocumentStatistics -> Task

type StubDocumentStatisticsRepository() =

    interface IDocumentStatisticsRepository with
        member this.SetAsync(stats: DocumentStatistics) =
            task {
                // TODO:
                return 0
            }

type MongoDocumentStatisticsRepository(config: IOptions<AppConfiguration>, logFactory: ILoggerFactory) =
    
    [<Literal>]
    let colName = "DocumentStatistics"

    let log = logFactory.CreateLogger<MongoDocumentStatisticsRepository>()

    let collection =
        Mongo.initCollection "uri" config.Value.mongoDbName colName config.Value.mongoConnection
        |> Mongo.setIndex "wordFrequencies"

    interface IStatsSource with
        member this.GetStatsAsync() =
            task {
                let! count = Mongo.estimatedCount collection

                return
                    { Stats.name = this.GetType().Name
                      itemCount = count
                      childStats = [] }
            }

    interface IDocumentStatisticsRepository with
        member this.SetAsync(stats: DocumentStatistics) =
            task {
                // TODO:
                return 0
            }