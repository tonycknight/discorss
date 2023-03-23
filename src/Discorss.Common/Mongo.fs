namespace Discorss

open System
open System.Diagnostics.CodeAnalysis
open MongoDB.Driver

[<ExcludeFromCodeCoverage>]
module Mongo =

    let dbClient (connection: string) =
        let settings = connection |> MongoClientSettings.FromConnectionString

        settings.AllowInsecureTls <- false
        settings.UseTls <- true
        settings.ConnectTimeout <- TimeSpan.FromSeconds(15)
        settings.ServerSelectionTimeout <- settings.ConnectTimeout

        new MongoClient(settings) :> IMongoClient

    let db (client: IMongoClient) name = client.GetDatabase name
