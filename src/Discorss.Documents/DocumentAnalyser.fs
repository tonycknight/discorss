namespace Discorss.Documents

open System
open Discorss

type IDocumentAnalyser =
    abstract member GetStatistics: Document -> DocumentStatistics
    abstract member GetWords: Document -> seq<string>

type DocumentAnalyser(lexicon: ILexicon) =

    let words (doc: Document) =
        let wordify = Tokenisation.wordify lexicon

        (wordify doc.content)
        |> Seq.append (wordify doc.description)
        |> Seq.append (wordify doc.author)
        |> Seq.append (wordify doc.title)
        |> Seq.map String.lower
        |> Seq.filter ((String.IsNullOrEmpty >> not) &&>> (lexicon.IsStopWord >> not))

    interface IDocumentAnalyser with
        member this.GetWords(doc: Document) = words doc

        member this.GetStatistics(doc: Document) =
            let wordCounts = doc |> words |> Seq.counts
            let wordCount = wordCounts |> Seq.sumBy (fun kvp -> kvp.Value)

            { DocumentStatistics.uri = doc.uri
              wordCount = wordCount
              wordFrequencies = wordCounts }
