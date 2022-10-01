namespace Discorss.Test.Benchmarks

open System
open BenchmarkDotNet.Running

module Program=

    [<EntryPoint>]
    let main (args: string[]) =

        try
            let asm = typedefof<Indexing.IndexingBenchmarks>.Assembly
            BenchmarkSwitcher.FromAssembly(asm).Run(args) |> ignore
            0
        with
        | ex -> 
            Console.ForegroundColor <- ConsoleColor.Red
            ex.ToString() |> Console.WriteLine
            Console.ResetColor()
            1