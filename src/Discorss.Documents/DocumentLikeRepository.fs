namespace Discorss.Documents

open System
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Options
open MongoDB.Bson

type IDocumentLikeRepository =
    abstract member GetAsync: string -> Task<DocumentLike option>
    abstract member SetAsync: DocumentLike -> Task<DocumentLike>
    abstract member DeleteAsync: string -> Task
    abstract member GetLikeUris: bool -> Task<string list>

[<ExcludeFromCodeCoverage>]
type StubDocumentLikeRepository() =
    interface IDocumentLikeRepository with
        member this.GetAsync(uri: string) = None |> Task.ofResult
        member this.SetAsync(value: DocumentLike) = value |> Task.ofResult
        member this.DeleteAsync(uri: string) = task { ignore 0 }
        member this.GetLikeUris(like: bool) = [] |> Task.ofResult


type MongoDocumentLikeRepository(config: IOptions<AppConfiguration>) =
    [<Literal>]
    let colName = "DocumentLikes"

    let collection =
        Mongo.initCollection "uri" config.Value.mongoDbName colName config.Value.mongoConnection

    let getStatsUris (like: bool) =
        let getString key doc =
            doc |> MongoBson.getProperty key |> MongoBson.asString

        let rec read (acc: string list) (cursor: MongoDB.Driver.IAsyncCursor<obj>) =
            match cursor.MoveNext() with
            | false -> acc
            | true ->
                let counts =
                    cursor.Current
                    |> Seq.map (fun x -> x.ToBsonDocument())
                    |> Seq.map (fun d -> d |> getString "_id")
                    |> List.ofSeq

                let acc = acc |> List.append counts
                read acc cursor

        task {

            let pipeline =
                [| sprintf "{ $match: { liked: %b } }" like; "{ $project: { _id: 1 } }" |]
                |> Mongo.pipeline

            use cursor = collection.Aggregate pipeline
            return read [] cursor |> List.ofSeq
        }

    interface IStatsSource with
        member this.GetStatsAsync() =
            task {
                let! count = Mongo.estimatedCount collection

                return
                    { Stats.name = this.GetType().Name
                      itemCount = count
                      childStats = [] }
            }

    interface IDocumentLikeRepository with
        member this.GetAsync(uri: string) =
            task {
                let! xs = $"{{ _id: '{String.lower uri}' }}" |> Mongo.getMany<BsonDocument> collection

                return xs |> Seq.map BsonMapping.fromDocumentLikeBson |> Seq.tryHead
            }

        member this.SetAsync(value: DocumentLike) =
            task {
                let! result = value |> BsonMapping.toDocumentLikeBson |> Mongo.upsert collection

                if not result.IsAcknowledged then
                    new Exception("Set not acknowledged") |> raise

                return value
            }

        member this.DeleteAsync(uri: string) =
            task {
                let! r = $"{{ _id: '{String.lower uri}' }}" |> Mongo.delete collection
                ignore r
            }

        member this.GetLikeUris(like: bool) = getStatsUris like
