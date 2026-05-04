namespace Discorss

open System
open MongoDB.Bson
open MongoDB.Bson.Serialization
open MongoDB.Driver

module MongoBson =
    let newObject () = new BsonDocument()

    let value (value: 'a) = BsonValue.Create(value)

    let setProperty (key: string) (value: BsonValue) (doc: BsonDocument) =
        doc.[key] <- value
        doc
    
    let getProperty (key: string) (doc: BsonDocument) = doc.[key]

    let asString (value: BsonValue) = value.AsString

    let asDateTimeOffset (value: BsonValue) = value.AsBsonDateTime.ToUniversalTime() |> DateTimeOffset

    let asStringArray (value: BsonValue) = value.AsBsonArray |> Seq.map (fun x -> x.AsString) |> Array.ofSeq

    let objectId () = ObjectId.GenerateNewId()

    let ofJson (json: string) =
        BsonSerializer.Deserialize<BsonDocument>(json)

    let toObject<'a> (doc: BsonDocument) =
        BsonSerializer.Deserialize<'a>(doc)

    let setDocId (id) (doc: BsonDocument) =
        let existingId = doc.Elements |> Seq.filter (fun e -> e.Name = "_id") |> Seq.tryHead

        match existingId with
        | None ->
            doc["_id"] <- id
            doc
        | _ -> doc

    let getDocId (bson: BsonDocument) =
        bson.Elements |> Seq.filter (fun e -> e.Name = "_id") |> Seq.head

    let getObjectId (bson: BsonDocument) =
        bson |> getDocId |> (fun id -> id.Value.AsObjectId)

    let getId (bson: BsonDocument) =
        bson |> getDocId |> (fun id -> id.Value.AsString)

module Mongo =
    let private idFilter id = sprintf @"{ _id: ""%s"" }" id

    let setDbConnection dbName (connectionString: string) =
        if String.IsNullOrWhiteSpace dbName then
            connectionString
        else
            $"""{connectionString |> Strings.appendIfMissing "/"}{dbName}"""

    let initDb dbName (connection: string) =
        let client = new MongoClient(connection)
        let db = client.GetDatabase(dbName)

        try
            new MongoDB.Driver.BsonDocumentCommand<Object>(BsonDocument.Parse("{ping:1}"))
            |> db.RunCommand
            |> ignore

            db
        with :? System.TimeoutException as ex ->
            raise (
                new ApplicationException "Cannot connect to DB. Check the server name, credentials & firewalls are correct."
            )

    let setIndex (path: string) (collection: IMongoCollection<'a>) =
        let json = sprintf "{'%s': 1 }" path
        let def = IndexKeysDefinition<'a>.op_Implicit (json)
        let model = CreateIndexModel<'a>(def)
        let r = collection.Indexes.CreateOne(model)

        collection

    let getCollection colName (db: IMongoDatabase) = db.GetCollection(colName)

    let initCollection indexPath dbName collectionName connectionString =
        let col =
            connectionString
            |> setDbConnection dbName
            |> initDb dbName
            |> getCollection collectionName

        if indexPath <> "" then col |> setIndex indexPath else col

    let upsert (collection: IMongoCollection<BsonDocument>) (doc: BsonDocument) =
        let opts = ReplaceOptions()
        opts.IsUpsert <- true

        let filter =
            doc
            |> MongoBson.getId
            |> idFilter
            |> MongoBson.ofJson
            |> FilterDefinition.op_Implicit

        collection.ReplaceOneAsync(filter, doc, opts)

    let query<'a> (collection: IMongoCollection<BsonDocument>) =
        collection.AsQueryable<BsonDocument>() |> Seq.map MongoBson.toObject<'a>

    let getMany<'a> (collection: IMongoCollection<BsonDocument>) (predicate: string) =
        task {
            let fieldFilter = new JsonFilterDefinition<BsonDocument>(predicate)
            use! r = collection.FindAsync(fieldFilter)

            return r.ToEnumerable() 
                    |> Seq.map MongoBson.toObject<'a> 
                    |> Array.ofSeq
        }

    let estimatedCount (collection: IMongoCollection<BsonDocument>) =
        collection.EstimatedDocumentCountAsync()

