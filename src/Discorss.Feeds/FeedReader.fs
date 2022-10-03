namespace Discorss.Feeds

open System
open System.Xml.Linq
open Discorss

module FeedReader =
    
    let parseBodyToXml body=
        try
            XDocument.Parse(body) |> Some
        with
        | :? System.Xml.XmlException -> None


    let parser (xml: XDocument) = 
        match xml with
        | AtomParser.IsAtom x ->  AtomParser.parse
        | _ -> (fun x y -> None)

    let parseXmlToFeed url (xml: XDocument) =
        (parser xml) url xml

    let read (clients: IExternalHttpClientFactory) url =
        task {
            let client = clients.httpClient("")

            let! resp = client.get url

            // TODO: errors/unkonwn feeds
            return match resp with
                    | HttpOkRequestResponse(status,body) -> body |> parseBodyToXml |> Option.bind (parseXmlToFeed url) |> Choice1Of2
                    | HttpErrorRequestResponse(status,body) -> Choice2Of2 (new Exception(body)) // TODO: better error discrimination
                    | HttpExceptionRequestResponse(ex) -> Choice2Of2 ex
        }
        
