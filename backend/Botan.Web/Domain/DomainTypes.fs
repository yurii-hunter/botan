namespace Botan.Web.Domain

open Botan.Web.Domain.Errors

// Simple types
module ConstrainedType =
    let createString fieldName minLen maxLen str =
        match str with
        | null -> AppError.createResult (NullField fieldName)
        | "" -> AppError.createResult (EmptyField fieldName)
        | t when t.Length > maxLen -> AppError.createResult (TooLongValue(fieldName, maxLen))
        | t when t.Length < minLen -> AppError.createResult (TooShortValue(fieldName, minLen))
        | _ -> Result.Ok str

type Slug = Slug of string

type EntityId = EntityId of int

module EntityId =
    let create id = EntityId id

    let value (EntityId id) = id

// user
type UserRole =
    | User
    | Admin
    | SuperAdmin

module UserRole =

    let create (name: string) =
        match name.ToLower() with
        | "user" -> Result.Ok UserRole.User
        | "admin" -> Result.Ok UserRole.Admin
        | "superadmin" -> Result.Ok UserRole.SuperAdmin
        | role -> AppError.createResult (UnknownUserRole role)

    let toString (role: UserRole) = role.ToString()

type Email = Email of string

module Email =
    let create str =
        ConstrainedType.createString "Email" 10 100 str
        |> Result.map Email

    let value (Email str) = str

type Password = Password of string

module Password =
    let create str =
        ConstrainedType.createString "Password" 6 20 str
        |> Result.map Password

    let value (Password str) = str

type HashedPassword = HashedPassword of string

module HashedPassword =
    let create str =
        ConstrainedType.createString "HashedPassword" 10 100 str
        |> Result.map HashedPassword

    let value (HashedPassword str) = str

type UserName = UserName of string

module UserName =
    let create str =
        ConstrainedType.createString "UserName" 5 100 str
        |> Result.map UserName

    let value (UserName str) = str

type UnvalidatedUserRegistration =
    { Name: string
      Email: string
      Password: string
      PasswordConfirmation: string }

type ValidatedUserRegistration =
    { Name: UserName
      Email: Email
      Password: Password
      Role: UserRole }

type UserRegistrationWithHashedPassword =
    { Name: UserName
      Email: Email
      HashedPassword: HashedPassword
      Role: UserRole }

type UnvalidatedUserLogin = { Email: string; Password: string }

type ValidatedUserLogin = { Email: Email; Password: Password }

type User =
    { Id: EntityId
      Name: UserName
      Email: Email
      HashedPassword: HashedPassword
      Role: UserRole }

// university
type UniversityName = UniversityName of string

module UniversityName =
    let create str =
        ConstrainedType.createString "UniversityName" 10 100 str
        |> Result.map UniversityName

    let value (UniversityName str) = str

type UnvalidatedUniversity = { Name: string }

type ValidatedUniversity = { Name: UniversityName }

type University = { Id: EntityId; Name: UniversityName }

// course
type CourseName = CourseName of string

module CourseName =
    let create str =
        ConstrainedType.createString "CourseName" 10 100 str
        |> Result.map CourseName

    let value (CourseName str) = str

type UnvalidatedCourse = { Name: string }

type ValidatedCourse = { Name: CourseName }

type Course = { Id: EntityId; Name: CourseName }

// task
type Title = Title of string

module Title =
    let create str =
        ConstrainedType.createString "Title" 10 100 str
        |> Result.map Title

    let value (Title str) = str

type Description = Description of string

module Description =
    let create str =
        ConstrainedType.createString "Description" 10 1_000 str
        |> Result.map Description

    let value (Description str) = str

type UnvalidatedTask = { Title: string; Description: string }

type ValidatedTask =
    { Title: Title
      Description: Description }

type Task =
    { Id: EntityId
      Title: Title
      Description: Description }

// solution
type Code = Code of string

module Code =
    let create code =
        ConstrainedType.createString "Code" 0 10_000 code
        |> Result.map Code

    let value (Code str) = str

type Language =
    | Pascal
    | CSharp
    | JavaScript
    | Java

module Language =
    let create langName =
        let name = String.toLowerInvariant langName

        match name with
        | "pascal" -> Result.Ok Language.Pascal
        | "csharp" -> Result.Ok Language.CSharp
        | "javascript" -> Result.Ok Language.JavaScript
        | "java" -> Result.Ok Language.Java
        | _ -> AppError.createResult (UnknownLanguage name)

    let toString (lang: Language) = lang.ToString()

type UnvalidatedSolution = { Code: string; Language: string }

type ValidatedSolution = { Code: Code; Language: Language }

type Solution =
    { Id: EntityId
      Code: Code
      Language: Language }

// query
type UnvalidatedQuery =
    { Solved: bool option
      Languages: string list }

type ValidatedQuery =
    { Solved: bool option
      Languages: Language list }
