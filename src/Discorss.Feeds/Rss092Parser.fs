namespace Discorss.Feeds

open System
open System.Xml.Linq

module Rss092Parser=
        
    let parseChannel (xml: XDocument)=
        match xml |> Xml.docElement "channel" with 
        | Some channel -> let title = channel |> Xml.elementValueDefault "title" 
                          let description = channel |> Xml.elementValueDefault "description" 
                          (title, description)
        | _ -> ("", "")

    let parseEntries (xml: XDocument)=
        let parse (e: XElement) =            
            { FeedEntry.id = e |> Xml.elementValueDefault "link";
                        publication = DateTimeOffset.Now;
                        uri = e |> Xml.elementValueDefault "link" ;
                        title = e |> Xml.elementValueDefault "title" ;
                        description = e |> Xml.elementValueDefault "description" ;
                        author = e |> Xml.elementValueDefault (Xml.xn XmlNs.dcns "creator") ;
                        content = e |> Xml.elementValueDefault (Xml.xn XmlNs.contentns "encoded") ;
                        categories = e |> Xml.elementValues "category" |> List.ofSeq;
                        }
        
        xml |> Xml.docElements "item" |> Seq.map parse |> List.ofSeq
        
    let isMatch (xml: XDocument) = 
        match Rss.rssVersion xml with
        | Some "0.92" -> true
        | _ -> false
    
    let parse url (xml: XDocument)=        
        
        let title, description = parseChannel xml
        
        let result = { Feed.uri = url; 
                            feedType = FeedType.Rss20; 
                            title = title; 
                            description = description; 
                            updated = DateTimeOffset.UtcNow; 
                            entries = parseEntries xml }
        result |> Some

    let (|IsRss092|_|) (xml: XDocument) =
        match xml |> isMatch with
        | true -> Some true
        | _ -> None
