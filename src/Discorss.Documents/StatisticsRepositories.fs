namespace Discorss.Documents

open System
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Options
open MongoDB.Bson

type IDocumentStatisticsRepository =
    abstract member SetAsync: DocumentStatistics -> Task<DocumentStatistics>
    abstract member GetAsync: string -> Task<DocumentStatistics option>

type StubDocumentStatisticsRepository() =

    interface IDocumentStatisticsRepository with
        member this.SetAsync(stats: DocumentStatistics) =
            task {
                return stats
            }
        member this.GetAsync(uri: string) =
            task { return None }

type MongoDocumentStatisticsRepository(config: IOptions<AppConfiguration>) =
    
    [<Literal>]
    let colName = "DocumentStatistics"

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
        member this.SetAsync(value: DocumentStatistics) =
            task {
                let! result = value |> BsonMapping.toDocumentStatisticsBson |> Mongo.upsert collection
                
                if not result.IsAcknowledged then
                    new Exception("Set not acknowledged") |> raise

                return value
            }

        member this.GetAsync(uri: string) =
            task { 
                let! xs = $"{{ _id: '{uri}' }}" |> Mongo.getMany<BsonDocument> collection

                return xs |> Seq.map BsonMapping.fromDocumentStatisticsBson |> Seq.tryHead
            }