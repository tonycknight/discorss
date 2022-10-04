namespace Discorss.Feeds

open System
open System.Xml.Linq

module Rss20Parser=
        
    let parseChannel (xml: XDocument)=
        match xml |> Xml.docElement "channel" with 
        | Some channel -> let title = channel |> Xml.elementValueDefault "title" |> Rss.dehtmlify
                          let description = channel |> Xml.elementValueDefault "description" |> Rss.dehtmlify
                          (title, description)
        | _ -> ("", "")

    let parseEntries (xml: XDocument)=
        let parse (e: XElement) =            
            { FeedEntry.id = e |> Xml.elementValueDefault "link";
                        publication = DateTimeOffset.Now;
                        uri = e |> Xml.elementValueDefault "link" ;
                        title = e |> Xml.elementValueDefault "title" |> Rss.dehtmlify;
                        description = e |> Xml.elementValueDefault "description" |> Rss.dehtmlify;
                        author = e |> Xml.elementValueDefault (Xml.xn XmlNs.dcns "creator") ;
                        content = e |> Xml.elementValueDefault (Xml.xn XmlNs.contentns "encoded") |> Rss.dehtmlify;
                        categories = e |> Xml.elementValues "category" |> List.ofSeq;
                        }
        
        xml |> Xml.docElements "item" |> Seq.map parse |> List.ofSeq
        
    let isMatch (xml: XDocument) = 
        match Rss.rssVersion xml with
        | Some "2.0" -> true
        | _ -> false
    
    let parse url (xml: XDocument)=        
        
        let title, description = parseChannel xml
        
        let result = { Feed.uri = url; 
                            feedType = FeedType.Rss20; 
                            title = title |> Rss.dehtmlify;
                            description = description |> Rss.dehtmlify; 
                            updated = DateTimeOffset.UtcNow; 
                            entries = parseEntries xml }
        result |> Some

    let (|IsRss20|_|) (xml: XDocument) =
        match xml |> isMatch with
        | true -> Some true
        | _ -> None
