namespace Discorss.Documents.Test.Unit

open System
open Discorss.Documents
open FsCheck.Xunit
open FsUnit.Xunit

module BsonMappingTests =

    [<Property>]
    let ``toBson / fromBson is symmetric`` (doc: Document) =
        
        let result = doc |> BsonMapping.toBson |> BsonMapping.fromBson

        result.uri |> should equal doc.uri
        result.description |> should equal doc.description
        result.content |> should equal doc.content
        result.author |> should equal doc.author
        result.sha512 |> should equal doc.sha512
        result.title |> should equal doc.title
        result.publication |> should equal doc.publication
        result.categories |> should equalSeq doc.categories

        true
    