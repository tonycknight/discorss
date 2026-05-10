namespace Discorss.Ingestion

open Discorss
open Discorss.Documents
open Discorss.Feeds

module Models =
    let sha512 (value: FeedEntry) =
        let xs =
            seq {
                yield value.author
                yield value.title
                yield value.description
                yield value.content
                yield! value.categories
            }
            |> Strings.join ""

        Strings.sha512 xs

    let toDocument (value: FeedEntry) =
        { Document.uri = value.uri
          author = value.author
          publication = value.publication
          title = value.title
          description = value.description
          content = value.content
          categories = value.categories
          sha512 = sha512 value }
