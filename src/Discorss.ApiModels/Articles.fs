namespace Discorss.ApiModels

open System

[<CLIMutable>]
type ArticleRequest =
    { uri: string
      title: string
      publication: DateTime
      description: string
      author: string
      content: string
      categories: string[] }
