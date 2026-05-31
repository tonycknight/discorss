namespace Discorss.Tests.Integration.Documents

open Discorss
open Discorss.Documents
open Discorss.Tests.Integration
open FsCheck.Xunit

module MongoDocumentLikesRepositoryTests =

    [<Property(Arbitrary = [| typeof<AlphaNumericString> |])>]
    let ``SetAsync / GetAsync are symmetric`` (value: DocumentLike) =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let value =
                { value with
                    uri = value.uri + (System.Guid.NewGuid().ToString()) }

            let repo = new MongoDocumentLikeRepository(opts) :> IDocumentLikeRepository

            let! result = repo.SetAsync value

            let! persistedResult = repo.GetAsync value.uri
            let persistedResult = Option.get persistedResult

            return result = value && value = persistedResult
        }

    [<Property(Arbitrary = [| typeof<AlphaNumericString> |])>]
    let ``GetAsync is not case sensitive`` (value: DocumentLike) =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let value =
                { value with
                    uri = value.uri + (System.Guid.NewGuid().ToString()) }

            let repo = new MongoDocumentLikeRepository(opts) :> IDocumentLikeRepository

            let! result = repo.SetAsync value

            let! upperResult = value.uri |> Strings.upper |> repo.GetAsync
            let! lowerResult = value.uri |> Strings.lower |> repo.GetAsync
            let! persistedResult = repo.GetAsync value.uri

            return
                result = value
                && value = Option.get upperResult
                && value = Option.get lowerResult
                && value = Option.get persistedResult
        }

    [<Property(Arbitrary = [| typeof<AlphaNumericString> |])>]
    let ``DeleteAsync / GetAsync returns None`` (value: DocumentLike) =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let value =
                { value with
                    uri = value.uri + (System.Guid.NewGuid().ToString()) }

            let repo = new MongoDocumentLikeRepository(opts) :> IDocumentLikeRepository

            let! result = repo.SetAsync value

            do! value.uri |> Strings.upper |> repo.DeleteAsync

            let! persistedResult = repo.GetAsync value.uri

            return persistedResult = None
        }

    [<Property(Arbitrary = [| typeof<AlphaNumericString> |])>]
    let ``GetLikeUris returns list`` (value: DocumentLike) (like: bool) =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let value =
                { value with
                    uri = value.uri + (System.Guid.NewGuid().ToString()) |> Strings.lower }

            let repo = new MongoDocumentLikeRepository(opts) :> IDocumentLikeRepository

            let! result = repo.SetAsync value

            let! uris = repo.GetLikeUris like

            let isMatch = uris |> List.contains value.uri

            return isMatch = (like = value.liked)

        }
