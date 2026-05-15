namespace Discorss.Documents.Service

open System
open Discorss.ApiModels
open Discorss.Documents

module Mapping =

    let toDocumentApiModel (value: Discorss.Documents.Document) =
        { Discorss.ApiModels.Document.uri = value.uri
          title = value.title
          description = value.description
          content = value.content
          author = value.author
          publication = value.publication |> DateTimeOffset
          categories = value.categories }

    let toDocumentLikeApiModel (value: Discorss.Documents.DocumentLike) =
        { Discorss.ApiModels.DocumentLike.uri = value.uri
          liked = value.liked }

    let fromDocumentLikeApiModel (value: Discorss.ApiModels.DocumentLike) =
        { Discorss.Documents.DocumentLike.uri = value.uri; liked = value.liked }
