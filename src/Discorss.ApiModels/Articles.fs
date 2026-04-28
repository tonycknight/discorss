namespace Discorss.ApiModels

[<CLIMutable>]
type ArticleRequest =
    { uri: string
      title: string
      description: string
      author: string
      content: string
      categories: string[] }
