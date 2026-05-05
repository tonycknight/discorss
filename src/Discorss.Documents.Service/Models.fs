namespace Discorss.Documents.Service

open System
open Discorss.ApiModels

module Mapping =

    let toDocumentApiModel (value: Discorss.Documents.Document) =
        { Discorss.ApiModels.Document.uri = value.uri
          title = value.title
          description = value.description
          content = value.content
          author = value.author
          publication = value.publication |> DateTimeOffset
          categories = value.categories }
