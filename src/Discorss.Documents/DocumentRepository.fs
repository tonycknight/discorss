namespace Discorss.Documents

open System.Threading.Tasks

type IDocumentRepository =
    abstract member SetDocumentAsync: Document -> Task<unit>
    abstract member GetDocumentAsync: string -> Task<Document option>

type StubDocumentRepository() =

    interface IDocumentRepository with
        member this.SetDocumentAsync(value: Document) = task { ignore 0 }

        member this.GetDocumentAsync(value: string) = task { return None }
