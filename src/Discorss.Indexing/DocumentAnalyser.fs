namespace Discorss.Indexing

open Discorss

type IDocumentAnalyser=
    abstract member Statistics: Document -> DocumentStatistics

type DocumentAnalyser()=
    
    let words (doc: Document)=
        let titleWords = doc.title |> Tokenisation.wordify
        let descWords = doc.description |> Tokenisation.wordify
        let contentWords = doc.content |> Tokenisation.wordify
        let authorWords = doc.author |> Tokenisation.wordify
                    
        titleWords
            |> Seq.append descWords
            |> Seq.append contentWords
            |> Seq.append authorWords
            |> Seq.filter (Lexicons.isStopWord >> not)

    interface IDocumentAnalyser with
        member this.Statistics(doc: Document)=
            { DocumentStatistics.uri = doc.uri; 
                                 wordFrequencies = doc |> words |> Seq.counts |> Array.ofSeq; 
                                 }
