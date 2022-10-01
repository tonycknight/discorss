namespace Discorss.Test.Benchmarks

open System
open BenchmarkDotNet.Running;

module Program=
    [<EntryPoint>]
    let main (args: string[]) =

        try
            //BenchmarkDotNet.Running.BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            0
        with
        | ex -> 
            Console.ForegroundColor <- ConsoleColor.Red
            Console.WriteLine(ex.ToString())
            Console.ResetColor()
            1