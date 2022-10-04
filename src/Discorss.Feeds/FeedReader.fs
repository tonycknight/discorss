namespace Discorss.Feeds

open System
open System.Xml.Linq
open Discorss

module FeedReader =
    
    let private parseBodyToXml body =
        try
            XDocument.Parse(body) |> FeedReadResult.Xml
        with
        | :? System.Xml.XmlException as ex -> FeedReadResult.Error ex.Message
            
    let private parser (xml: XDocument) = 
        match xml with
        | Rss20Parser.IsRss20 x ->      Rss20Parser.parse |> Some
        | Rss092Parser.IsRss092 x ->    Rss092Parser.parse |> Some
        | Rss091Parser.IsRss091 x ->    Rss091Parser.parse |> Some
        | _ -> None
                
    let private parseXmlToFeed url (xml: XDocument) =
        match parser xml with
        | Some p -> (p url xml) |> Option.map FeedReadResult.Feed
                                |> Option.defaultValue ( FeedReadResult.Error "Error in parsing")
        | None -> FeedReadResult.Error "No parser found"
        
    let parse url body =
        body    |> parseBodyToXml
                |> (function
                    | FeedReadResult.Xml xml -> parseXmlToFeed url xml
                    | x -> x)                

    let readAsync (client: IExternalHttpClient) url =
        task {            
            let! resp = client.GetAsync url

            return match resp with
                    | HttpOkRequestResponse(status,body) ->     body |> parse url
                    | HttpErrorRequestResponse(status,body) ->  FeedReadResult.Error (sprintf "HTTP %A received" status)
                    | HttpExceptionRequestResponse(ex) ->       FeedReadResult.Error ex.Message
        }
        
