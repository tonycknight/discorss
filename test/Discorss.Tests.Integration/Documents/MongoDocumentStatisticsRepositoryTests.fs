namespace Discorss.Tests.Integration.Documents

open Discorss
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

    [<Property(Arbitrary = [| typeof<AlphaNumericString> |])>]
    let ``GetAggregatedStatsAsync produces aggregated stats`` (values: DocumentStatistics[]) =
        task {
            let opts = TestHelpers.config () |> TestHelpers.configOptions

            let repo =
                new MongoDocumentStatisticsRepository(opts) :> IDocumentStatisticsRepository

            let words = [ ("aaa", 1); ("bbb", 3) ] |> Map.ofSeq

            let values =
                values
                |> Array.map (fun v ->
                    { v with
                        wordFrequencies = words
                        uri = v.uri + (System.Guid.NewGuid().ToString()) })

            let! values = values |> Array.map repo.SetAsync |> Task.whenAll

            let! stats = values |> Array.map _.uri |> repo.GetAggregatedStatsAsync

            let expected =
                if values.Length = 0 then
                    Map.empty
                else
                    words |> Seq.map (fun kvp -> (kvp.Key, kvp.Value * values.Length)) |> Map.ofSeq

            return
                stats.Count = expected.Count
                && (stats |> Seq.sumBy (fun kvp -> kvp.Value)) = (expected |> Seq.sumBy (fun kvp -> kvp.Value))
        }
