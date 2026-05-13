namespace Discorss.Tests.Integration.Documents

open Discorss.Documents
open Discorss.Tests.Integration
open FsCheck.Xunit

module MongoDocumentStatisticsRepositoryTests =

    [<Property(Arbitrary = [| typeof<AlphaNumericString> |])>]
    let ``SetAsync / GetAsync are symmetric`` (value: DocumentStatistics) =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let value =
                { value with
                    uri = value.uri + (System.Guid.NewGuid().ToString()) }

            let repo =
                new MongoDocumentStatisticsRepository(opts) :> IDocumentStatisticsRepository

            let! result = repo.SetAsync value

            let! persistedResult = repo.GetAsync value.uri

            return result = value && value = Option.get persistedResult
        }
