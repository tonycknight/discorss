namespace Discorss

open System
open System.Diagnostics.CodeAnalysis
open System.Threading.Tasks

[<AutoOpen>]
module Combinators =

    let (&&>>) x y = (fun (v: 'a) -> x (v) && y (v))

    let (||>>) x y = (fun (v: 'a) -> x (v) || y (v))

module ReturnCodes =

    [<Literal>]
    let ok = 0

    [<Literal>]
    let validationFailed = 1

    [<Literal>]
    let sysError = 2

module Strings =

    let toLower (value: string) = value.ToLower()

    let isEmptyWhitespace (value: string) = String.IsNullOrWhiteSpace value

    let isUri (value: string) =
        Uri.TryCreate(value, UriKind.Absolute) |> fst

    let join (delim: string) (strings: seq<string>) = String.Join(delim, strings)

    let indent (pad: int) (value: string) = $"{new String(' ', pad)}{value}"

    let truncate (maxLength: int) (value: string) =
        if value.Length <= maxLength then
            value
        else
            value.Substring(0, maxLength - 3) + "..."

    let escapeMarkup (value: string) =
        value.Replace("[", "[[").Replace("]", "]]")

    let fromGzip (value: System.IO.Stream) =
        let bufferSize = 512
        let buffer = Array.create<byte> bufferSize 0uy
        use outStream = new System.IO.MemoryStream()

        use decomp =
            new System.IO.Compression.GZipStream(value, System.IO.Compression.CompressionMode.Decompress)

        let mutable len = -1

        while len <> 0 do
            len <- decomp.Read(buffer)

            if len > 0 then
                outStream.Write(buffer, 0, len)

        outStream.Seek(0, System.IO.SeekOrigin.Begin) |> ignore
        use reader = new System.IO.StreamReader(outStream)
        reader.ReadToEnd()

module internal Option =

    let ofNull<'a> (value: 'a) =
        if Object.ReferenceEquals(value, null) then
            None
        else
            Some value

module internal Tasks =
    let toTaskResult (value) =
        System.Threading.Tasks.Task.FromResult(value)

module Environment =

    [<ExcludeFromCodeCoverage>]
    let isRunningGithub =
        System.Environment.GetEnvironmentVariable("GITHUB_ACTIONS") <> null

module Exception =

    let catchDefault (defaultValue: 'b) (func: unit -> Task<'b>) =
        task {
            try
                let! r = func ()
                return r
            with ex ->
                return defaultValue
        }

module Process =
    open System.Diagnostics

    let openUri (uri: string) =
        try
            let ps = new ProcessStartInfo(uri)
            ps.LoadUserProfile <- true
            ps.UseShellExecute <- true

            use x = Process.Start ps

            true
        with ex ->
            false
