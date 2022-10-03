namespace Discorss.Feeds

open System
open System.Xml.Linq

module AtomParser=
        
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
                        url = e |> Xml.elementValueDefault "link" ;
                        title = e |> Xml.elementValueDefault "title" ;
                        description = e |> Xml.elementValueDefault "description" ;
                        author = e |> Xml.elementValueDefault (Xml.xn XmlNs.dcns "creator") ;
                        content = e |> Xml.elementValueDefault (Xml.xn XmlNs.contentns "encoded") ;
                        categories = e |> Xml.elementValues "category" |> List.ofSeq;
                        }
        
        xml |> Xml.docElements "item" |> Seq.map parse |> List.ofSeq
        
    let isAtom (xml: XDocument) = 
        match Rss.rssVersion xml with
        | Some "2.0" -> true
        | _ -> false
    
    let parse url (xml: XDocument)=        
        
        let title, description = parseChannel xml
        
        let result = { Feed.url = url; 
                            feedType = FeedType.Atom; 
                            title = title; 
                            description = description; 
                            updated = DateTimeOffset.UtcNow; 
                            entries = parseEntries xml }
        result |> Some

    let (|IsAtom|_|) (xml: XDocument) =
        xml |> isAtom |> Some
