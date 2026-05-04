namespace Discorss.Tests.Integration.Documents

open System
open Discorss.Documents
open Discorss.Tests.Integration
open FsUnit.Xunit

module MongoDocumentRepositoryTests =

    [<Xunit.Fact>]
    let ``SetDocumentAsync writes one`` () =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let repo =
                new MongoDocumentRepository(opts, TestHelpers.logFactory ()) :> IDocumentRepository

            let document =
                { Document.uri = $"http://localhost/{Guid.NewGuid()}"
                  publication = DateTime.UtcNow
                  author = "test author name"
                  title = "test doc title"
                  description = "test description"
                  content = "test content"
                  categories = [| "tag1"; "tag2" |]
                  sha512 = "test sha" }

            let! result = repo.SetDocumentAsync document

            result.uri |> should equal document.uri
        }

    [<Xunit.Fact>]
    let ``SetDocumentAsync updates one`` () =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let repo =
                new MongoDocumentRepository(opts, TestHelpers.logFactory ()) :> IDocumentRepository

            let document =
                { Document.uri = $"http://localhost/{Guid.NewGuid()}"
                  publication = DateTime.UtcNow
                  author = "test author name"
                  title = "test doc title"
                  description = "test description"
                  content = "test content"
                  categories = [| "tag1"; "tag2" |]
                  sha512 = "test sha" }

            let! result = repo.SetDocumentAsync document

            let document =
                { document with
                    description = Guid.NewGuid().ToString()
                    title = Guid.NewGuid().ToString()
                    content = Guid.NewGuid().ToString()
                    author = Guid.NewGuid().ToString() }

            let! result = repo.SetDocumentAsync document

            let! persistedDocument = repo.GetDocumentAsync document.uri

            persistedDocument.Value.uri |> should equal document.uri
            persistedDocument.Value.description |> should equal document.description
            persistedDocument.Value.title |> should equal document.title
            persistedDocument.Value.content |> should equal document.content
            persistedDocument.Value.author |> should equal document.author
            persistedDocument.Value.sha512 |> should equal document.sha512

            persistedDocument.Value.publication.ToLongTimeString()
            |> should equal (document.publication.ToLongTimeString())

            persistedDocument.Value.categories |> should equalSeq document.categories
        }
