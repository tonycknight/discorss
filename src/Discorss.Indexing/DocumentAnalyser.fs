namespace Discorss.Indexing

open System
open Discorss

type IDocumentAnalyser =
    abstract member Statistics: Document -> DocumentStatistics
    abstract member Words: Document -> seq<string>

type DocumentAnalyser(lexicon: ILexicon) =

    let words (doc: Document) =
        let wordify = Tokenisation.wordify lexicon

        (wordify doc.content)
        |> Seq.append (wordify doc.description)
        |> Seq.append (wordify doc.author)
        |> Seq.append (wordify doc.title)
        |> Seq.map Strings.lower
        |> Seq.filter ((String.IsNullOrEmpty >> not) &&>> (lexicon.IsStopWord >> not))

    interface IDocumentAnalyser with
        member this.Words(doc: Document) = words doc

        member this.Statistics(doc: Document) =
            let wordCounts = doc |> words |> Seq.counts |> Map.ofSeq
            let wordCount = wordCounts |> Seq.sumBy (fun kvp -> kvp.Value)

            { DocumentStatistics.uri = doc.uri
              wordCount = wordCount
              wordFrequencies = wordCounts }
