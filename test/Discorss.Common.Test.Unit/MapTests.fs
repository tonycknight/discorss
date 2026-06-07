namespace Discorss.Common.Test.Unit

open Discorss
open FsCheck
open FsCheck.Xunit

module MapTests =

    [<Property(Verbose = true)>]
    let ``add adds two maps`` (ints: PositiveInt[]) =
        
        let xs = ints |> Array.map (fun i -> (i.Get.ToString(), i.Get))

        let x = xs |> Map.ofSeq
        let y = xs |> Map.ofSeq

        let result = Map.add x y
        
        result 
        |> Seq.map (fun kvp -> kvp.Value = (x.[kvp.Key] + y.[kvp.Key]) )
        |> Seq.forall id
        