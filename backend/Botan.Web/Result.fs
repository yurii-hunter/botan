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
        let (<*>) = Result.apply
        let (<!>) = Result.map
        let cons head tail = head :: tail
        let consR headR tailR = cons <!> headR <*> tailR
        let initialValue = Ok [] // empty list inside Result

        // loop through the list, prepending each element
        // to the initial value
        List.foldBack consR aListOfValidations initialValue

    type ResultBuilder() =
        member this.Return x = Result.Ok x
        member this.Bind(x, f) = Result.bind f x

    let result = ResultBuilder()
