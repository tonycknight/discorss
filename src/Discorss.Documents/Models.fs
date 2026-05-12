namespace Discorss.Documents

open System

type Document =
    { uri: string
      publication: DateTime
      author: string
      title: string
      description: string
      content: string
      categories: string[]
      sha512: string }


type DocumentStatistics =
    { uri: string
      wordCount: int
      wordFrequencies: Map<string, int> }

type WordStatistics = { word: string; wordCounts: int }


module BsonMapping =
    open Discorss.MongoBson
    open MongoDB.Bson

    let toBson (document: Document) =
        newObject ()
        |> setDocId (value document.uri)
        |> setProperty "uri" (value document.uri)
        |> setProperty "title" (value document.title)
        |> setProperty "content" (value document.content)
        |> setProperty "description" (value document.description)
        |> setProperty "author" (value document.author)
        |> setProperty "sha512" (value document.sha512)
        |> setProperty "publication" (valueDate document.publication)
        |> setProperty "categories" (value document.categories)
        |> setProperty "updated" (value DateTime.UtcNow)

    let fromBson (document: BsonDocument) =
        let asString key = getProperty key >> asString

        { Document.uri = document |> asString "uri"
          title = document |> asString "title"
          content = document |> asString "content"
          description = document |> asString "description"
          author = document |> asString "author"
          sha512 = document |> asString "sha512"
          publication = document |> getProperty "publication" |> asDateTime
          categories = document |> getProperty "categories" |> asStringArray }
