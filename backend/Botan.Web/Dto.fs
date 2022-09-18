module Botan.Web.Dto

open Botan.Web.Domain
open Suave.Http
open Suave.Utils

type UniversityFormDto = { Name: string }

module UniversityFormDto =
    let toUnvalidatedUniversity (universityFormDto: UniversityFormDto) =
        { UnvalidatedUniversity.Name = universityFormDto.Name }

type UniversityDto = { Id: string; Name: string }

module UniversityDto =
    let fromUniversity (university: University) =
        { Id = EntityId.value university.Id
          Name = UniversityName.value university.Name }

type CategoryFormDto = { Name: string }

module CategoryFormDto =
    let toUnvalidatedCategory (categoryFormDto: CategoryFormDto) =
        { UnvalidatedCategory.Name = categoryFormDto.Name }

type CategoryDto = { Id: string; Name: string }

module CategoryDto =
    let fromCategory (category: Category) =
        { Id = EntityId.value category.Id
          Name = CategoryName.value category.Name }

type QuestionFormDto =
    { Title: string
      University: string
      Category: string
      Description: string }

module QuestionFormDto =
    let toUnvalidatedQuestion questionFormDto =
        { UnvalidatedQuestion.Title = questionFormDto.Title
          University = questionFormDto.University
          Category = questionFormDto.Category
          Description = questionFormDto.Description }

type AnswerFormDto = { Code: string; Language: string }

module AnswerFormDto =
    let toUnvalidatedAnswer answerFormDto =
        { UnvalidatedAnswer.Code = answerFormDto.Code
          UnvalidatedAnswer.Language = answerFormDto.Language }

type AnswerDto =
    { Id: string
      Code: string
      Language: string }

module AnswerDto =
    let fromAnswer (answer: Answer) =
        { Id = EntityId.value answer.Id
          AnswerDto.Code = Code.value answer.Code
          Language = Language.value answer.Language }

type QuestionDto =
    { Id: string
      Title: string
      Description: string
      University: UniversityDto
      Category: CategoryDto
      Answers: AnswerDto seq }

module QuestionDto =
    let fromQuestion (question: Question) =
        { Id = EntityId.value question.Id
          Title = Title.value question.Title
          Description = Description.value question.Description
          University = UniversityDto.fromUniversity question.University
          Category = CategoryDto.fromCategory question.Category
          Answers = question.Answers |> Seq.map AnswerDto.fromAnswer }

    let fromQuestions questions = questions |> List.map fromQuestion

module QueryDto =
    let toUnvalidatedQuery (request: HttpRequest) =
        let answered =
            request.queryParam "answered"
            |> Choice.map bool.Parse
            |> Option.ofChoice

        let languages =
            request.query
            |> List.filter (fun q -> fst q = "lang")
            |> List.choose snd

        { UnvalidatedQuery.Answered = answered
          Languages = languages }
