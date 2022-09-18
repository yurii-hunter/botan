module Botan.Web.Implementation

open System
open Botan.Web.Domain
open FSharp.Core
open Botan.Web.Extensions.Result

// validation
let validateUniversity (unvalidatedUniversity: UnvalidatedUniversity) =
    UniversityName.create unvalidatedUniversity.Name
    |> Result.map (fun name -> { ValidatedUniversity.Name = name })

let validateCategory (unvalidatedCategory: UnvalidatedCategory) =
    CategoryName.create unvalidatedCategory.Name
    |> Result.map (fun name -> { ValidatedCategory.Name = name })

let validateQuestion (unvalidatedTask: UnvalidatedQuestion) =
    result {
        let! title = Title.create unvalidatedTask.Title
        let! description = Description.create unvalidatedTask.Description
        let! universityId = EntityId.create unvalidatedTask.University
        let! categoryId = EntityId.create unvalidatedTask.Category

        return
            { Title = title
              University = universityId
              Category = categoryId
              Description = description
              Created = DateTime.Now }
    }

let validateAnswer (unvalidatedAnswer: UnvalidatedAnswer) =
    result {
        let! code = Code.create unvalidatedAnswer.Code
        let! language = Language.create unvalidatedAnswer.Language

        return
            { ValidatedAnswer.Code = code
              Language = language }
    }

let validateQuery (unvalidatedQuery: UnvalidatedQuery) =
    result {
        let answered = unvalidatedQuery.Answered

        let! languages =
            unvalidatedQuery.Languages
            |> List.map Language.create
            |> sequence

        return
            { ValidatedQuery.Answered = answered
              Languages = languages }
    }

// actions
let createUniversity addUniversityToStore unvalidatedUniversity =
    unvalidatedUniversity
    |> validateUniversity
    |> Result.map addUniversityToStore

let createCategory addCategoryToStore unvalidatedCategory =
    unvalidatedCategory
    |> validateCategory
    |> Result.map addCategoryToStore

let createQuestion storeQuestionInDb unvalidatedQuestion =
    unvalidatedQuestion
    |> validateQuestion
    |> Result.bind storeQuestionInDb

let addAnswer addAnswerToStore questionId unvalidatedAnswer =
    unvalidatedAnswer
    |> validateAnswer
    |> Result.bind (addAnswerToStore questionId)

let getQuestionById (getQuestionFromStore: 'a -> Result<Question, string>) questionId =
    questionId |> getQuestionFromStore

let getAllQuestions getAllTasksFromStore unvalidatedQuery =
    unvalidatedQuery
    |> validateQuery
    |> Result.bind getAllTasksFromStore
