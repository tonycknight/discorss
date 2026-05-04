namespace Discorss.Tests.Integration.Documents

open System
open Discorss
open Discorss.Documents
open Discorss.Tests.Integration
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open NSubstitute
open FsUnit.Xunit

module MongoDocumentRepositoryTests =
            
    [<Xunit.Fact>]
    let ``SetDocumentAsync``() =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let repo = new MongoDocumentRepository(opts, TestHelpers.logFactory()) :> IDocumentRepository

            let document = 
                { Document.uri = $"http://localhost/{Guid.NewGuid()}"
                  publication = DateTimeOffset.UtcNow
                  author = "test author name"
                  title = "test doc title"
                  description = "test description"
                  content = "test content"
                  categories = [| "tag1"; "tag2" |]
                  sha512 = ""
                }

            let! result = repo.SetDocumentAsync document
            
            result.uri |> should equal document.uri
        }