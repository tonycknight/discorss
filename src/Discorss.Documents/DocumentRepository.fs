namespace Discorss.Documents

open System
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open MongoDB.Bson

type IDocumentRepository =
    abstract member SetDocumentAsync: Document -> Task<Document>
    abstract member GetDocumentAsync: string -> Task<Document option>

[<ExcludeFromCodeCoverage>]
type StubDocumentRepository() =

    interface IDocumentRepository with
        member this.SetDocumentAsync(value: Document) = task { return value }

        member this.GetDocumentAsync(value: string) = task { return None }

type MongoDocumentRepository(config: IOptions<AppConfiguration>, logFactory: ILoggerFactory) =

    [<Literal>]
    let colName = "Documents"

    let log = logFactory.CreateLogger<MongoDocumentRepository>()

    let collection =
        Mongo.initCollection "uri" config.Value.mongoDbName colName config.Value.mongoConnection
        |> Mongo.setIndex "publication"
        |> Mongo.setIndex "categories"

    interface IDocumentRepository with
        member this.SetDocumentAsync(value: Document) =
            task {
                let! result = value |> BsonMapping.toBson |> Mongo.upsert collection

                if not result.IsAcknowledged then
                    new Exception("Set not acknowledged") |> raise

                return value
            }

        member this.GetDocumentAsync(key: string) =
            task {
                let! xs = $"{{ _id: '{key}' }}" |> Mongo.getMany<BsonDocument> collection

                return xs |> Seq.map BsonMapping.fromBson |> Seq.tryHead
            }
