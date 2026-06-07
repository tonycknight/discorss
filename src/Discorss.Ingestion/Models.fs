namespace Discorss.Ingestion

open Discorss
open Discorss.Documents
open Discorss.Feeds

module Models =
    let sha512 (value: FeedEntry) =
        value.author + value.title + value.description + value.content |> String.sha512

    let toDocument (value: FeedEntry) =
        { Document.uri = value.uri
          author = value.author
          publication = value.publication
          title = value.title
          description = value.description
          content = value.content
          categories = value.categories |> Array.map String.lower
          sha512 = sha512 value }
