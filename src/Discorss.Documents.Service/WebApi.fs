namespace Discorss.Documents.Service

open System
open Discorss
open Giraffe

module WebApi =

    let webApp path (sp: IServiceProvider) =

        subRouteCi
            path
            (Api.logClient
             >=> choose
                     [ GET
                       >=> choose [ route "/" >=> (noResponseCaching >=> WebAppHandlers.getNextDocument sp) ] ])
