namespace Botan.Web.Extensions

module Result =
    let getOk result =
        match result with
        | Ok v -> v
        | Error _ -> failwith "result is error"

    let getError result =
        match result with
        | Ok _ -> failwith "result is not error"
        | Error er -> er

    let sequence (aListOfValidations: Result<_, _> list) =
        let folder state acc =
            match state, acc with
            | Ok v, Ok acc -> Ok(v :: acc)
            | Error e, Ok _ -> Error e
            | Ok _, Error e -> Error e
            | Error e1, Error _ -> Error e1

        List.foldBack folder aListOfValidations (Ok [])

    type ResultBuilder() =
        member this.Return x = Result.Ok x
        member this.Bind(x, f) = Result.bind f x
        member this.ReturnFrom(x) = x

    let result = ResultBuilder()
