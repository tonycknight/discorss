namespace Discorss

[<AutoOpen>]
module Combinators=

    let (>&&>) x y = 
        (fun (v: 'a) -> x(v) && y(v))

    let (>||>) x y = 
        (fun (v: 'a) -> x(v) || y(v))

module Task=
    open System.Threading.Tasks

    let map(f: 'a -> 'b) (x: Task<'a>)=
        x.GetAwaiter().GetResult() |> f // TODO: ewww        

