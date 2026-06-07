namespace Discorss.Documents

open System
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Options
open MongoDB.Bson

type IDocumentStatisticsRepository =
    abstract member SetAsync: DocumentStatistics -> Task<DocumentStatistics>
    abstract member GetAsync: string -> Task<DocumentStatistics option>
    abstract member GetAggregatedStatsAsync: string seq -> Task<Map<string, int>>

type StubDocumentStatisticsRepository() =

    interface IDocumentStatisticsRepository with
        member this.SetAsync(stats: DocumentStatistics) = task { return stats }
        member this.GetAsync(uri: string) = task { return None }
        member this.GetAggregatedStatsAsync uris = Map.empty |> Task.ofResult

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
                let! xs = $"{{ _id: '{Strings.lower uri}' }}" |> Mongo.getMany<BsonDocument> collection

                return xs |> Seq.map BsonMapping.fromDocumentStatisticsBson |> Seq.tryHead
            }

        member this.GetAggregatedStatsAsync uris =
            let getString key doc =
                    doc |> MongoBson.getProperty key |> MongoBson.asString

            let getInt32 key doc =
                doc |> MongoBson.getProperty key |> MongoBson.asInt32

            let rec read (acc) (cursor: MongoDB.Driver.IAsyncCursor<obj>) =
                match cursor.MoveNext() with
                | false -> acc
                | true ->
                    let counts =
                        cursor.Current
                        |> Seq.map (fun x -> x.ToBsonDocument())
                        |> Seq.map (fun d -> (d |> getString "_id", d |> getInt32 "count"))
                        |> Map.ofSeq
                    let acc = acc |> Map.add counts
                    read acc cursor

            task {

                let uris = uris |> Seq.map (fun x -> $"\"{Strings.lower x}\"") |> Strings.join ", "

                let pipeline =
                    [| sprintf "{ $match: { _id: { $in: [ %s ] }}}" uris
                       "{ $project: { words: { $objectToArray: \"$wordFrequencies\" } } }"
                       "{ $unwind: { path: \"$words\" } }"
                       "{ $group: { _id: \"$words.k\", count: { $sum: \"$words.v\" } } }" |]
                    |> Mongo.pipeline

                return pipeline |> collection.Aggregate |> read Map.empty
            }
