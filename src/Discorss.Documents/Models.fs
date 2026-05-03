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
