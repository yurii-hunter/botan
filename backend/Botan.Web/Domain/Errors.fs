namespace Botan.Web.Domain

module Errors =

    type ValidationError =
        // Input validation
        | NullField of name: string
        | EmptyField of name: string
        | TooLongValue of name: string * maxLen: int
        | TooShortValue of name: string * minLen: int
        | UnknownLanguage of lng: string

    type DataStoreError =
        | RecordNotFound of ntt: string * query: obj
        | DuplicateRecord of ex: exn
        | DataBaseError of ex: exn

    type DomainError =
        | PasswordConfirmation
        | InvalidCredentials

    type AppError =
        | Validation of ValidationError
        | DataStore of DataStoreError
        | Domain of DomainError

        static member create(e: ValidationError) = Validation e
        static member create(e: DataStoreError) = DataStore e
        static member create(e: DomainError) = Domain e

        static member createResult(e: ValidationError) = Error(Validation e)
        static member createResult(e: DataStoreError) = Error(DataStore e)
        static member createResult(e: DomainError) = Error(Domain e)

        static member createResult(e: AppError) =
            match e with
            | Validation error -> AppError.createResult error
            | DataStore error -> AppError.createResult error
            | Domain error -> AppError.createResult error

        static member toString(error: ValidationError) =
            match error with
            | NullField name -> $"{name} must not be null"
            | EmptyField name -> $"{name} must not be empty"
            | TooLongValue (name, maxLen) -> $"{name} must not be longer than {maxLen} characters"
            | TooShortValue (name, minLen) -> $"{name} must not be shorter than {minLen} characters"
            | UnknownLanguage lng -> $"Unknown language: {lng}"

        static member toString(error: DataStoreError) =
            match error with
            | RecordNotFound (ntt, query) -> $"Entity {ntt} ({query}) is not found"
            | DataBaseError ex -> $"Error executing query: %A{ex}"
            | DuplicateRecord ex -> $"Duplicated record: %A{ex}"

        static member toString(error: DomainError) =
            match error with
            | PasswordConfirmation -> "Password and password confirmation do not match"
            | InvalidCredentials -> "Invalid email or password"
