namespace Discorss

open System.Xml.Linq

module XmlNs =
    [<Literal>]
    let dcns = "http://purl.org/dc/elements/1.1/"

    [<Literal>]
    let contentns = "http://purl.org/rss/1.0/modules/content/"

module Xml =
    let xn (ns: string) (name: string) = XName.Get(name, ns)

    let docName (doc: XDocument) =
        match doc.Root |> Option.ofNull |> Option.map _.Name with
        | Some n ->
            n.LocalName
            |> Option.ofNull
            |> Option.map String.lower
            |> Option.defaultValue ""
        | _ -> ""

    let docElements name (doc: XDocument) =
        doc.Descendants() |> Seq.filter (fun e -> e.Name.LocalName = name)

    let docElement name (doc: XDocument) = doc |> docElements name |> Seq.tryHead

    let elements name (value: XElement) =
        value.Elements() |> Seq.filter (fun e -> e.Name.LocalName = name)

    let element name (value: XElement) = value |> elements name |> Seq.tryHead

    let elementValue name (value: XElement) =
        value |> element name |> Option.map (fun a -> a.Value)

    let elementValueDefault name (value: XElement) =
        value |> elementValue name |> Option.defaultValue ""

    let elementValues name (value: XElement) =
        value |> elements name |> Seq.map (fun a -> a.Value)

    let attribute name (element: XElement) = element.Attributes(name) |> Seq.tryHead

    let attributeValue name (element: XElement) =
        element |> attribute name |> Option.map (fun a -> a.Value)
