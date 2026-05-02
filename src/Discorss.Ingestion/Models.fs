namespace Discorss.Ingestion

open Discorss.Documents
open Discorss.Feeds

module Models =
    let toDocument (value: FeedEntry) =
        { Document.uri = value.uri
          author = value.author
          publication = value.publication
          title = value.title
          description = value.description
          content = value.content
          categories = value.categories }