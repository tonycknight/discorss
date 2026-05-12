namespace Discorss.Indexing.Service

open System
open Giraffe

module WebApp =

    let webApp path (sp: IServiceProvider) =

        subRouteCi
            path
            (choose
                [ POST
                  >=> choose
                          [ route "/stats" >=> WebAppHandlers.getDocumentStatistics sp
                            route "/words" >=> WebAppHandlers.getDocumentWords sp ]
                  PUT >=> choose [ route "/index" >=> WebAppHandlers.setDocument sp ] ])
