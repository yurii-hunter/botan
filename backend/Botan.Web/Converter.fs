module Botan.Web.Converter

open System

module Converter =

    let private mapSymbol symbol =
        match symbol with
        | 'а' -> 'a'
        | 'б' -> 'b'
        | ' ' -> '-'
        | _ -> '-'

    let toSlug (str: string) : string =
        str |> Seq.map mapSymbol |> Array.ofSeq |> String
