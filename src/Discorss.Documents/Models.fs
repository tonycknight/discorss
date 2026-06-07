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

type DocumentLike = { uri: string; liked: bool }

module BsonMapping =
    open Discorss
    open Discorss.MongoBson
    open MongoDB.Bson

    let toDocumentBson (document: Document) =
        newObject ()
        |> setDocId (document.uri |> String.lower |> value)
        |> setProperty "uri" (value document.uri)
        |> setProperty "title" (value document.title)
        |> setProperty "content" (value document.content)
        |> setProperty "description" (value document.description)
        |> setProperty "author" (value document.author)
        |> setProperty "sha512" (value document.sha512)
        |> setProperty "publication" (valueDate document.publication)
        |> setProperty "categories" (value document.categories)
        |> setProperty "updated" (value DateTime.UtcNow)

    let fromDocumentBson (document: BsonDocument) =
        let asString key = getProperty key >> asString

        { Document.uri = document |> asString "uri"
          title = document |> asString "title"
          content = document |> asString "content"
          description = document |> asString "description"
          author = document |> asString "author"
          sha512 = document |> asString "sha512"
          publication = document |> getProperty "publication" |> asDateTime
          categories = document |> getProperty "categories" |> asStringArray }

    let toDocumentStatisticsBson (stats: DocumentStatistics) =
        newObject ()
        |> setDocId (stats.uri |> String.lower |> value)
        |> setProperty "uri" (value stats.uri)
        |> setProperty "wordCount" (value stats.wordCount)
        |> setProperty "wordFrequencies" (stats.wordFrequencies |> Dictionary.ofMap |> value)

    let fromDocumentStatisticsBson (document: BsonDocument) =
        let asString key = getProperty key >> asString
        let asInt key = getProperty key >> asInt32
        let asDocument key = getProperty key >> asDocument

        let freqs =
            document
            |> asDocument "wordFrequencies"
            |> _.ToDictionary()
            |> Seq.map (fun kvp -> (kvp.Key, kvp.Value :?> int32))
            |> Map.ofSeq

        { DocumentStatistics.uri = document |> asString "uri"
          wordCount = document |> asInt "wordCount"
          wordFrequencies = freqs }

    let toDocumentLikeBson (document: DocumentLike) =
        newObject ()
        |> setDocId (document.uri |> String.lower |> value)
        |> setProperty "uri" (value document.uri)
        |> setProperty "liked" (value document.liked)

    let fromDocumentLikeBson (document: BsonDocument) =
        { DocumentLike.uri = document |> getProperty "uri" |> asString
          liked = document |> getProperty "liked" |> asBoolean }
