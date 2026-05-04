namespace Discorss.Documents

open System.Threading.Tasks
open Discorss
open Microsoft.Extensions.Options

type IDocumentRepository =
    abstract member SetDocumentAsync: Document -> Task<Document>
    abstract member GetDocumentAsync: string -> Task<Document option>

type StubDocumentRepository() =

    interface IDocumentRepository with
        member this.SetDocumentAsync(value: Document) = task { return value }

        member this.GetDocumentAsync(value: string) = task { return None }

type MongoDocumentRepository(config: IOptions<AppConfiguration>) =
    // TODO: 
        
    interface IDocumentRepository with
        member this.SetDocumentAsync(value: Document) = task { return value }

        member this.GetDocumentAsync(value: string) = task { return None }