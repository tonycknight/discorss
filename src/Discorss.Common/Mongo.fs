namespace Discorss

open System
open System.Diagnostics.CodeAnalysis
open MongoDB.Driver

[<ExcludeFromCodeCoverage>]
module Mongo =
    let private idFilter id = sprintf @"{ _id: ""%s"" }" id

    let dbClient (connection: string) =
        let settings = connection |> MongoClientSettings.FromConnectionString

        settings.AllowInsecureTls <- false
        settings.UseTls <- true
        settings.ConnectTimeout <- System.TimeSpan.FromSeconds(15.0)
        settings.ServerSelectionTimeout <- settings.ConnectTimeout

        new MongoClient(settings) :> IMongoClient

    let db (client: IMongoClient) name = client.GetDatabase name
