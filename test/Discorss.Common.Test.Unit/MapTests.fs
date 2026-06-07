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
        |> Seq.map (fun kvp -> kvp.Value = (x.[kvp.Key] + y.[kvp.Key]))
        |> Seq.forall id


    [<Property(Verbose = true)>]
    let ``addMany folds aggrgated value`` (ints: PositiveInt[]) =
        let xs = ints |> Array.map (fun i -> ("aaa", i.Get))

        let maps = xs |> Array.map (fun kvp -> Map.ofSeq [ kvp ])

        let result = Map.empty |> Map.addMany maps

        (result |> Seq.sumBy _.Value) = (ints |> Array.sumBy _.Get)

    [<Property(Verbose = true)>]
    let ``addMany folds into single key`` (ints: PositiveInt[]) =
        let xs = ints |> Array.map (fun i -> ("aaa", i.Get))

        let maps = xs |> Array.map (fun kvp -> Map.ofSeq [ kvp ])

        let result = Map.empty |> Map.addMany maps

        match ints with
        | [||] -> true
        | _ -> result.Count = 1 && (result.["aaa"] = (ints |> Array.sumBy _.Get))
