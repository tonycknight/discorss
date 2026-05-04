namespace Discorss.Documents

open System
open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

type IDocumentRepository =
    abstract member SetDocumentAsync: Document -> Task<Document>
    abstract member GetDocumentAsync: string -> Task<Document option>

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

    let toBson (document: Document) =        
        MongoBson.newObject ()
        |> MongoBson.setDocId (MongoBson.value document.uri)
        |> MongoBson.property "uri" (MongoBson.value document.uri)
        |> MongoBson.property "title" (MongoBson.value document.title)
        |> MongoBson.property "content" (MongoBson.value document.content)
        |> MongoBson.property "description" (MongoBson.value document.description)
        |> MongoBson.property "author" (MongoBson.value document.author)
        |> MongoBson.property "sha512" (MongoBson.value document.sha512)
        |> MongoBson.property "publication" (MongoBson.value document.publication.DateTime)
        |> MongoBson.property "categories" (MongoBson.value document.categories)
        
    //let fromBson (document: BsonDocument) =


    interface IDocumentRepository with
        member this.SetDocumentAsync(value: Document) = 
            task { 
                let bson = toBson value

                let! result = Mongo.upsert collection bson
                // TODO: check result?
                return value 
            }

        member this.GetDocumentAsync(value: string) = task { return None }