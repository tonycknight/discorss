namespace Discorss.Feeds.Test.Unit

module TestHelpers =

    let testPath () =
        System.Reflection.Assembly.GetExecutingAssembly().Location
        |> System.IO.Path.GetDirectoryName

    let sampleFeedsPath () =
        System.IO.Path.Combine(testPath (), "SampleFeeds")

    let sampleFeeds () =
        let path = sampleFeedsPath ()

        System.IO.Directory.EnumerateFiles(path, "*.xml")
        |> Seq.map (fun p -> new System.IO.FileInfo(p))

    let sampleFeed name =
        let feeds = sampleFeeds ()

        feeds |> Seq.filter (fun fi -> fi.Name = name) |> Seq.head

    let sampleFeedAsString name =
        let feed = sampleFeed name
        use stream = feed.OpenRead()
        use rdr = new System.IO.StreamReader(stream)

        rdr.ReadToEnd()
