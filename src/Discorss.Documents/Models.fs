namespace Discorss.Documents

open System

type Document =
    { uri: string
      publication: DateTimeOffset
      author: string
      title: string
      description: string
      content: string
      categories: string[]
      sha512: string }

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
        |> setProperty "publication" (value document.publication.DateTime)
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
          publication = document |> getProperty "publication" |> asDateTimeOffset
          categories = document |> getProperty "categories" |> asStringArray }
