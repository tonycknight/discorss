namespace Discorss.Indexing


type IDocumentAnalyser=
    abstract member Analyse: Document -> DocumentStatistics

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
        member this.Analyse(doc: Document)=

            let wordCounts = doc    |> words
                                    |> Seq.groupBy id 
                                    |> Seq.map (fun (w,ws) -> (w, ws |> Seq.length) )
                                    |> Array.ofSeq
            { DocumentStatistics.uri = doc.uri; wordFrequencies = wordCounts; }

