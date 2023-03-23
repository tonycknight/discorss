namespace Discorss.Feeds

open System
open System.Xml.Linq

module XmlNs =
    [<Literal>]
    let dcns = "http://purl.org/dc/elements/1.1/"

    [<Literal>]
    let contentns = "http://purl.org/rss/1.0/modules/content/"

module Xml =
    let xn (ns: string) (name: string) = XName.Get(name, ns)

    let docElement name (doc: XDocument) = doc.Descendants(name) |> Seq.tryHead

    let docElements name (doc: XDocument) = doc.Descendants(name)

    let element name (value: XElement) = value.Elements(name) |> Seq.tryHead

    let elements name (value: XElement) = value.Elements(name)

    let elementValue name (value: XElement) =
        value |> element name |> Option.map (fun a -> a.Value)

    let elementValueDefault name (value: XElement) =
        value |> elementValue name |> Option.defaultValue ""

    let elementValues name (value: XElement) =
        value |> elements name |> Seq.map (fun a -> a.Value)

    let attribute name (element: XElement) = element.Attributes(name) |> Seq.tryHead

    let attributeValue name (element: XElement) =
        element |> attribute name |> Option.map (fun a -> a.Value)
