module Botan.Web.Api

open Botan.Web.Domain
open Botan.Web.Dto
open Newtonsoft.Json
open Suave
open Suave.Successful
open Suave.RequestErrors
open Suave.Operators
open Botan.Web.Implementation
open Botan.Web.Effects
open Botan.Web.Dto.AnswerFormDto
open Botan.Web.Dto.AnswerDto
open Botan.Web.Dto.QuestionFormDto
open Botan.Web.Dto.QuestionDto
open Botan.Web.Dto.UniversityDto
open Botan.Web.Dto.UniversityFormDto
open Botan.Web.Dto.CategoryFormDto
open Botan.Web.Dto.CategoryDto
open Botan.Web.Dto.QueryDto

let workflowResultToHttpResponse result =
    match result with
    | Ok r ->
        r |> JsonConvert.SerializeObject |> OK
        >=> Writers.setMimeType "application/json; charset=utf-8"
    | Error r -> BAD_REQUEST r

let createUniversity: WebPart =
    fun httpContext ->
        let createUniversityFlow = createUniversity (addUniversityToStore (getDBClient ()))

        System.Text.Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<UniversityFormDto>
        |> toUnvalidatedUniversity
        |> createUniversityFlow
        |> Result.map fromUniversity
        |> (fun result -> workflowResultToHttpResponse result httpContext)

let createCategory: WebPart =
    fun httpContext ->
        let createCategoryFlow = createCategory (addCategoryToStore (getDBClient ()))

        System.Text.Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<CategoryFormDto>
        |> toUnvalidatedCategory
        |> createCategoryFlow
        |> Result.map fromCategory
        |> (fun result -> workflowResultToHttpResponse result httpContext)

let createQuestion: WebPart =
    fun httpContext ->

        let createQuestionFlow = createQuestion (addQuestionToStore (getDBClient ()))

        System.Text.Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<QuestionFormDto>
        |> toUnvalidatedQuestion
        |> createQuestionFlow
        |> Result.map fromQuestion
        |> (fun result -> workflowResultToHttpResponse result httpContext)

let addAnswer (questionId: string) =
    fun httpContext ->

        let workflow = addAnswer (addAnswerToStore (getDBClient ()))

        System.Text.Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<AnswerFormDto>
        |> toUnvalidatedAnswer
        |> workflow (EntityId questionId)
        |> Result.map fromAnswer
        |> (fun result -> workflowResultToHttpResponse result httpContext)

let getQuestionById (id: string) =
    let getQuestionByIdFlow = getQuestionById (getQuestionFromStore (getDBClient ()))

    EntityId id
    |> getQuestionByIdFlow
    |> Result.map fromQuestion
    |> workflowResultToHttpResponse

let getAllQuestions =
    request (fun r ->

        let getAllQuestionsFlow =
            getAllQuestions (getAllQuestionsFromStore (getDBClient ()))

        r
        |> toUnvalidatedQuery
        |> getAllQuestionsFlow
        |> Result.map fromQuestions
        |> workflowResultToHttpResponse)
