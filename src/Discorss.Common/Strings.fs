namespace Discorss

open System
open System.Security.Cryptography

module Strings =
    let isEmptyWhitespace (value: string) = String.IsNullOrWhiteSpace value

    let str (value: 'a) = value.ToString()

    let trim (value: string) = value.Trim()

    let lower (text: string) = text.ToLowerInvariant()

    let upper (text: string) = text.ToUpperInvariant()

    let mixed (text: string) =
        let flip =
            let rng = new Random()
            fun () -> rng.Next(2) = 0

        text
        |> Seq.map (fun c ->
            match flip () with
            | true -> Char.ToUpper(c)
            | _ -> Char.ToLower(c))
        |> Array.ofSeq
        |> String

    let sha256 (value: string) =
        use x = SHA256.Create()
        let b = System.Text.Encoding.UTF8.GetBytes value

        let hash = x.ComputeHash(b)

        System.Convert.ToBase64String hash

    let sha512 (value: string) =
        use x = SHA512.Create()
        let b = System.Text.Encoding.UTF8.GetBytes value

        let hash = x.ComputeHash(b)

        System.Convert.ToBase64String hash

    let appendIfMissing (suffix: string) (value: string) =
        if value.EndsWith(suffix) |> not then
            $"{value}{suffix}"
        else
            value

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

    let editDistance x y =
        Tk.Extensions.StringExtensions.GetDamerauLevenshteinDistance(x, y)
