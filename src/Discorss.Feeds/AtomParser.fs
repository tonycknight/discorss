namespace Discorss.Feeds

open System
open System.Xml.Linq

module AtomParser=
    let dcns = "http://purl.org/dc/elements/1.1/"
    let contentns = "http://purl.org/rss/1.0/modules/content/"

    let xn (ns: string) (name: string) = XName.Get(name, ns)

    let docElement name (doc: XDocument) =
        doc.Descendants(name) |> Seq.tryHead

    let docElements name (doc: XDocument) =
        doc.Descendants(name) 
        
    let element name (value: XElement)=
        value.Elements(name) |> Seq.tryHead

    let elements name (value: XElement)=
        value.Elements(name)

    let elementValue name (value: XElement) = 
        value |> element name |> Option.map (fun a -> a.Value)

    let elementValues name (value: XElement) = 
        value |> elements name |> Seq.map (fun a -> a.Value)

    let attribute name (element: XElement)=
        element.Attributes(name) |> Seq.tryHead

    let attributeValue name (element: XElement) =        
        element |> attribute name |> Option.map (fun a -> a.Value)

    let hasRssAttribute(xml: XDocument) = 
        match xml |> docElement "rss" with
        | Some rss -> rss |> attribute "version" |> Option.isSome
        | None -> false

    let parseChannel (xml: XDocument)=
        match xml |> docElement "channel" with 
        | Some channel -> let title = channel |> elementValue "title" |> Option.defaultValue ""
                          let description = channel |> elementValue "description" |> Option.defaultValue ""
                          (title, description)
        | _ -> ("", "")

    let parseEntries (xml: XDocument)=
        let parse (e: XElement) =            
            { FeedEntry.id = e |> elementValue "link" |> Option.defaultValue "";
                        creation = DateTimeOffset.Now;
                        url = e |> elementValue "link" |> Option.defaultValue "";
                        title = e |> elementValue "title" |> Option.defaultValue "";
                        description = e |> elementValue "description" |> Option.defaultValue "";
                        author = e |> elementValue (xn dcns "creator") |> Option.defaultValue "";
                        content = e |> elementValue (xn contentns "encoded") |> Option.defaultValue "";
                        categories = e |> elementValues "category" |> List.ofSeq;
                        }
        
        xml |> docElements "item" |> Seq.map parse |> List.ofSeq
        
    let isAtom (xml: XDocument) = 
        hasRssAttribute xml
    
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
