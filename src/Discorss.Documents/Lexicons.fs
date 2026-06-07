namespace Discorss.Documents

open Discorss


type ILexicon =
    abstract member IsStopWord: string -> bool
    abstract member StopWords: unit -> seq<string>
    abstract member IsKnownWord: string -> bool
    abstract member KnownWords: unit -> seq<string>

type Lexicon() =
    let stopWords =
        [ "the"
          "these"
          "this"
          "a"
          "an"
          "and"
          "i"
          "we"
          "it"
          "is"
          "as"
          "be"
          "to"
          "has"
          "for" ]
        |> Set.ofSeq

    let knownWords = [ ".net"; "asp.net" ] |> Set.ofSeq

    let isStopWord word =
        stopWords |> Set.contains (String.lower word)

    let isKnownWord word =
        knownWords |> Set.contains (String.lower word)

    interface ILexicon with
        member this.IsStopWord(word: string) = isStopWord word
        member this.StopWords() = stopWords

        member this.IsKnownWord(word: string) = isKnownWord word
        member this.KnownWords() = knownWords
