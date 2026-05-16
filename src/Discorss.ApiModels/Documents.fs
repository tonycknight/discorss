namespace Discorss.ApiModels

open System

[<CLIMutable>]
type Document =
    { uri: string
      title: string
      publication: DateTimeOffset
      description: string
      author: string
      content: string
      categories: string[] }

[<CLIMutable>]
type DocumentLike = { uri: string; liked: bool }
