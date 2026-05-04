namespace Discorss.Documents

open System
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open MongoDB.Bson

type IDocumentRepository =
    abstract member SetDocumentAsync: Document -> Task<Document>
    abstract member GetDocumentAsync: string -> Task<Document option>

type StubDocumentRepository() =

    interface IDocumentRepository with
        member this.SetDocumentAsync(value: Document) = task { return value }

        member this.GetDocumentAsync(value: string) = task { return None }

module BsonMapping =
    let toBson (document: Document) =        
        MongoBson.newObject ()
        |> MongoBson.setDocId (MongoBson.value document.uri)
        |> MongoBson.setProperty "uri" (MongoBson.value document.uri)
        |> MongoBson.setProperty "title" (MongoBson.value document.title)
        |> MongoBson.setProperty "content" (MongoBson.value document.content)
        |> MongoBson.setProperty "description" (MongoBson.value document.description)
        |> MongoBson.setProperty "author" (MongoBson.value document.author)
        |> MongoBson.setProperty "sha512" (MongoBson.value document.sha512)
        |> MongoBson.setProperty "publication" (MongoBson.value document.publication.DateTime)
        |> MongoBson.setProperty "categories" (MongoBson.value document.categories)
    
    let fromBson (document: BsonDocument) =
        
        let result = 
            { Document.uri = document.["uri"].AsString
              title = document.["title"].AsString
              content = document.["content"].AsString
              description = document.["description"].AsString
              author = document.["author"].AsString
              sha512 = document.["sha512"].AsString
              publication = document.["publication"].AsBsonDateTime.ToUniversalTime() |> DateTimeOffset
              categories = document.["categories"].AsBsonArray |> Seq.map (fun x -> x.AsString) |> Array.ofSeq
            }

        result

type MongoDocumentRepository(config: IOptions<AppConfiguration>, logFactory: ILoggerFactory) =
    
    [<Literal>]
    let colName = "Documents"

    let log = logFactory.CreateLogger<MongoDocumentRepository>()

    let collection = 
        Mongo.initCollection "uri" config.Value.mongoDbName colName config.Value.mongoConnection
        |> Mongo.setIndex "publication"

    interface IDocumentRepository with
        member this.SetDocumentAsync(value: Document) = 
            task {                 
                let! result = value |> BsonMapping.toBson |> Mongo.upsert collection
                
                if not result.IsAcknowledged then
                    new Exception("Set not acknowledged") |> raise
                                
                return value 
            }

        member this.GetDocumentAsync(value: string) = 
            task { 
                let! xs = $"{{ _id: '{value}' }}" |> Mongo.getMany<BsonDocument> collection

                return 
                    xs 
                    |> Seq.map BsonMapping.fromBson
                    |> Seq.tryHead
            }