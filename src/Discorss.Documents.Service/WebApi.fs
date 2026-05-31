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
                       >=> choose
                               [ route "/queue/" >=> noResponseCaching >=> WebAppHandlers.getNextDocument sp
                                 routeCif "/likes/%s/" (fun uri -> uri |> WebAppHandlers.getDocumentLike sp)
                                 route "/categories/stats/"
                                 >=> noResponseCaching
                                 >=> WebAppHandlers.getCategoryStats sp ]

                       DELETE
                       >=> choose [ routeCif "/likes/%s/" (fun uri -> uri |> WebAppHandlers.deleteDocumentLike sp) ]

                       PUT >=> choose [ route "/likes/" >=> WebAppHandlers.setDocumentLike sp ] ])
