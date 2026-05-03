namespace Discorss.Feeds

open System.Threading.Tasks

type IFeedProvider =
    abstract member GetFeedAsync: string -> Task<FeedReadResult>

type FeedProvider(client: Discorss.IExternalHttpClient) =

    interface IFeedProvider with
        member this.GetFeedAsync(uri) = uri |> FeedReader.readAsync client
