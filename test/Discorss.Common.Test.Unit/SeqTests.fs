namespace Discorss.Feeds.Test.Unit

open System
open Discorss
open FsCheck
open FsCheck.Xunit
open FsUnit

module SeqTests=
    
    [<Xunit.Fact>]
    let ``noneToEmpty yields empty``()=        
        let r = None |> Seq.noneToEmpty

        r |> should equal Seq.empty

    [<Xunit.Fact>]
    let ``noneToEmpty yields sequence``()=
        let xs = seq { 1 } 

        let r = xs |> Some |> Seq.noneToEmpty
        
        r |> should equal xs


    [<Property(Verbose = true)>]
    let ``counts same value``(PositiveInt count)=
        
        let xs = [ 1 .. count ] |> Seq.map (fun _ -> 0) 

        let result = xs |> Seq.counts |> Array.ofSeq

        result |> should haveLength 1
        result |> Seq.head |> snd |> should equal count

        true

    [<Property(Verbose = true)>]
    let ``counts different values``(PositiveInt count)=
        
        let xs = [ 1 .. count ] |> Seq.collect (fun x -> [ x; x ]) 

        let result = xs |> Seq.counts |> Array.ofSeq

        result |> should haveLength count
        result |> Seq.map snd |> Seq.min |> should equal 2
        result |> Seq.map snd |> Seq.max |> should equal 2

        true

