module Botan.Web.Domain

open System

// Simple types
module ConstrainedType =
    let createString fieldName minLen maxLen str =
        match str with
        | null -> Result.Error $"{fieldName} must not be null"
        | "" -> Result.Error $"{fieldName} must not be empty"
        | t when t.Length > maxLen -> Result.Error $"{fieldName} must not be longer than {maxLen} characters"
        | t when t.Length < minLen -> Result.Error $"{fieldName} must not be shorter than {minLen} characters"
        | _ -> Result.Ok str

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

type EntityId = EntityId of string

module EntityId =
    let create str =
        ConstrainedType.createString "EntityId" 24 24 str
        |> Result.map EntityId

    let value (EntityId str) = str

type UniversityName = UniversityName of string

module UniversityName =
    let create str =
        ConstrainedType.createString "UniversityName" 10 100 str
        |> Result.map UniversityName

    let value (UniversityName str) = str

type CategoryName = CategoryName of string

module CategoryName =
    let create str =
        ConstrainedType.createString "CategoryName" 10 100 str
        |> Result.map CategoryName

    let value (CategoryName str) = str

type Code = Code of string

module Code =
    let create code =
        ConstrainedType.createString "Code" 0 10_000 code
        |> Result.map Code

    let value (Code str) = str

// Unvalidated types
type UnvalidatedUniversity = { Name: string }

type UnvalidatedCategory = { Name: string }

type UnvalidatedQuestion =
    { Title: string
      University: string
      Category: string
      Description: string }

type UnvalidatedAnswer = { Code: string; Language: string }

type UnvalidatedQuery =
    { Answered: bool option
      Languages: string list }

type UnvalidatedUser = { Email: string; Hash: string }

type UnvalidatedCredentials = { Email: string; Hash: string }

// ValidatedTypes
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
        | _ -> Result.Error $"Unknown language: {name}"

    let value lang = lang.ToString()

type ValidatedAnswer = { Code: Code; Language: Language }

type Answer =
    { Id: EntityId
      Code: Code
      Language: Language }

type ValidatedQuestion =
    { Title: Title
      University: EntityId
      Category: EntityId
      Description: Description
      Created: DateTime }

type ValidatedUniversity = { Name: UniversityName }

type ValidatedCategory = { Name: CategoryName }

type University = { Id: EntityId; Name: UniversityName }

type Category = { Id: EntityId; Name: CategoryName }

type Question =
    { Id: EntityId
      Title: Title
      University: University
      Category: Category
      Description: Description
      Created: DateTime
      Answers: Answer list }

type ValidatedQuery =
    { Answered: bool option
      Languages: Language list }
