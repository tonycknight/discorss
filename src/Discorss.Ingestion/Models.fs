namespace Discorss.Ingestion

open Discorss
open Discorss.Documents
open Discorss.Feeds

module Models =
    let sha512 (value: FeedEntry) =
        let xs =
            seq {
                value.author
                value.title
                value.description
                value.content
            }
            |> Strings.join ""

        Strings.sha512 xs

    let toDocument (value: FeedEntry) =
        { Document.uri = value.uri
          author = value.author
          publication = value.publication
          title = value.title |> Html.stripHtml
          description = value.description |> Html.stripHtml
          content = value.content |> Html.stripHtml
          categories = value.categories |> Seq.map Html.stripHtml |> Array.ofSeq
          sha512 = sha512 value }
