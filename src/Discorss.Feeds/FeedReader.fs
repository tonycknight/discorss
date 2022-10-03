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
        | Rss20Parser.IsRss20 x -> Some Rss20Parser.parse 
        | Rss092Parser.IsRss092 x -> Some Rss092Parser.parse 
        | Rss091Parser.IsRss091 x -> Some Rss091Parser.parse 
        | _ -> None

    let private parseXmlToFeed url (xml: XDocument) =
        match parser xml with
        | Some p -> (p url xml) |> Option.map FeedReadResult.Feed |> Option.defaultValue ( FeedReadResult.Error "Error in parsing")
        | None -> FeedReadResult.Error "No parser found"
        
    let parse url body =
        body    |> parseBodyToXml
                |> (function
                    | FeedReadResult.Xml xml -> parseXmlToFeed url xml
                    | x -> x)                

    let readAsync (clients: IExternalHttpClientFactory) url =
        task {
            let client = clients.GetHttpClient("")

            let! resp = client.GetAsync url

            return match resp with
                    | HttpOkRequestResponse(status,body) ->     body |> parse url
                    | HttpErrorRequestResponse(status,body) ->  FeedReadResult.Error (sprintf "HTTP %A received" status)
                    | HttpExceptionRequestResponse(ex) ->       FeedReadResult.Error ex.Message
        }
        
