namespace Discorss.Indexing

open System
open Discorss

type IDocumentAnalyser=
    abstract member Statistics: Document -> DocumentStatistics
    abstract member Words: Document -> seq<string>

type DocumentAnalyser(lexicon: ILexicon)=
    
    let words (doc: Document)=
        
        (Tokenisation.wordify doc.title)
            |> Seq.append (Tokenisation.wordify doc.description)
            |> Seq.append (Tokenisation.wordify doc.description)
            |> Seq.append (Tokenisation.wordify doc.author)
            |> Seq.map Strings.lower
            |> Seq.filter (String.IsNullOrEmpty >> not >&&> lexicon.IsStopWord >> not)

    interface IDocumentAnalyser with
        member this.Words(doc: Document)=
            words doc

        member this.Statistics(doc: Document)=
            { DocumentStatistics.uri = doc.uri; 
                                 wordFrequencies = doc |> words |> Seq.counts |> Array.ofSeq; 
                                 }
