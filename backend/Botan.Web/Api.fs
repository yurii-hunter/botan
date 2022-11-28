module Botan.Web.Api

open System.Text
open Botan.Web.Domain
open Botan.Web.Domain.Errors
open Botan.Web.Dto
open Newtonsoft.Json
open Suave
open Suave.Successful
open Suave.RequestErrors
open Suave.ServerErrors
open Suave.Operators
open Botan.Web.Implementation
open Botan.Web.Dto.SolutionFormDto
open Botan.Web.Dto.SolutionDto
open Botan.Web.Dto.TaskFormDto
open Botan.Web.Dto.TaskDto
open Botan.Web.Dto.UniversityDto
open Botan.Web.Dto.UniversityFormDto
open Botan.Web.Dto.CourseFormDto
open Botan.Web.Dto.CourseDto
open Botan.Web.Dto.UserRegistrationFormDto
open Botan.Web.Dto.UserDto
open Botan.Web.Dto.UserLoginFormDto
open Botan.Web.Extensions
open Botan.Web.Effects.UniversityStore
open Botan.Web.Effects.CourseStore
open Botan.Web.Effects.TaskStore
open Botan.Web.Effects.SolutionStore
open Botan.Web.Effects.UserStore
open Botan.Web.Auth

let workflowResultToHttpResponse result =
    match result with
    | Ok r ->
        r |> JsonConvert.SerializeObject |> OK
        >=> Writers.setMimeType "application/json; charset=utf-8"
    | Error (AppError.Validation validationError) -> BAD_REQUEST(AppError.toString validationError)
    | Error (AppError.DataStore dataStoreError) ->
        match dataStoreError with
        | RecordNotFound _ -> NOT_FOUND(AppError.toString dataStoreError)
        | DataBaseError _ ->
            printf $"%s{AppError.toString dataStoreError}"
            INTERNAL_ERROR "Database Error"
        | DuplicateRecord _ ->
            printf $"%s{AppError.toString dataStoreError}"
            BAD_REQUEST "Record already exists"
    | Error (AppError.Domain domainError) -> BAD_REQUEST(AppError.toString domainError)

// University
let createUniversity: WebPart =
    authorize Admin (fun httpContext ->
        let createUniversityFlow =
            createUniversity addUniversityToStore getUniversityFromStore

        Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<UniversityFormDto>
        |> toUnvalidatedUniversity
        |> createUniversityFlow
        |> Result.map fromUniversity
        |> (fun result -> workflowResultToHttpResponse result httpContext))

let getUniversity id =
    let getUniversityFlow = getUniversity getUniversityFromStore

    EntityId id
    |> getUniversityFlow
    |> Result.map fromUniversity
    |> workflowResultToHttpResponse

let getUniversities: WebPart =
    fun httpContext ->
        let getUniversitiesFlow = getUniversities getUniversitiesFromStore

        getUniversitiesFlow ()
        |> Result.map (List.map fromUniversity)
        |> (fun result -> workflowResultToHttpResponse result httpContext)

let updateUniversity (id: int) =
    authorize Admin (fun httpContext ->
        let updateUniversityFlow =
            updateUniversity updateUniversityInStore getUniversityFromStore

        Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<UniversityFormDto>
        |> toUnvalidatedUniversity
        |> updateUniversityFlow (EntityId.create id)
        |> Result.map fromUniversity
        |> (fun result -> workflowResultToHttpResponse result httpContext))

let deleteUniversity (id: int) =
    authorize SuperAdmin (fun httpContext ->
        let deleteUniversityFlow = deleteUniversity deleteUniversityFromStore

        id
        |> EntityId.create
        |> deleteUniversityFlow
        |> (fun result -> workflowResultToHttpResponse result httpContext))


// Course
let createCourse (universityId: int) : WebPart =
    authorize Admin (fun httpContext ->
        let createCourseFlow = createCourse addCourseToStore getCourseFromStore

        Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<CourseFormDto>
        |> toUnvalidatedCourse
        |> createCourseFlow (EntityId.create universityId)
        |> Result.map fromCourse
        |> (fun result -> workflowResultToHttpResponse result httpContext))

let getCourse (id: int) =
    let getCourseFlow = getCourse getCourseFromStore

    id
    |> EntityId.create
    |> getCourseFlow
    |> Result.map fromCourse
    |> workflowResultToHttpResponse

let getCourses (universityId: int) =
    let getCoursesFlow = getCourses getCoursesFromStore

    universityId
    |> EntityId.create
    |> getCoursesFlow
    |> Result.map (List.map fromCourse)
    |> workflowResultToHttpResponse

let updateCourse (id: int) =
    authorize Admin (fun httpContext ->
        let updateCourseFlow = updateCourse updateCourseInStore getCourseFromStore

        Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<CourseFormDto>
        |> toUnvalidatedCourse
        |> updateCourseFlow (EntityId.create id)
        |> Result.map fromCourse
        |> (fun result -> workflowResultToHttpResponse result httpContext))

let deleteCourse (id: int) =
    authorize SuperAdmin (fun httpContext ->
        let deleteCourseFlow = deleteCourse deleteCourseFromStore

        id
        |> EntityId.create
        |> deleteCourseFlow
        |> (fun result -> workflowResultToHttpResponse result httpContext))


// Task
let createTask (courseId: int) : WebPart =
    authorize Admin (fun httpContext ->

        let createTaskFlow = createTask addTaskToStore getTaskFromStore

        Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<TaskFormDto>
        |> toUnvalidatedTask
        |> createTaskFlow (EntityId.create courseId)
        |> Result.map fromTask
        |> (fun result -> workflowResultToHttpResponse result httpContext))

let getTasks (courseId: int) =
    let getTasksFlow = getTasks getTasksFromStore

    courseId
    |> EntityId.create
    |> getTasksFlow
    |> Result.map (List.map fromTask)
    |> workflowResultToHttpResponse

let getTask (id: int) =
    let getTaskFlow = getTask getTaskFromStore

    id
    |> EntityId.create
    |> getTaskFlow
    |> Result.map fromTask
    |> workflowResultToHttpResponse

let updateTask (id: int) =
    authorize Admin (fun httpContext ->
        let updateTaskFlow = updateTask updateTaskInStore getTaskFromStore

        Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<TaskFormDto>
        |> toUnvalidatedTask
        |> updateTaskFlow (EntityId.create id)
        |> Result.map fromTask
        |> (fun result -> workflowResultToHttpResponse result httpContext))

let deleteTask (id: int) =
    authorize SuperAdmin (fun httpContext ->
        let deleteTaskFlow = deleteTask deleteTaskFromStore

        id
        |> EntityId.create
        |> deleteTaskFlow
        |> (fun result -> workflowResultToHttpResponse result httpContext))


let createSolution (taskId: int) =
    authorize Admin (fun httpContext ->
        let createSolutionFlow = createSolution addSolutionToStore getSolutionFromStore

        Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<SolutionFormDto>
        |> toUnvalidatedSolution
        |> createSolutionFlow (EntityId.create taskId)
        |> Result.map fromSolution
        |> (fun result -> workflowResultToHttpResponse result httpContext))

let getSolutions (taskId: int) =
    let getSolutionsFlow = getSolutions getSolutionsFromStore

    taskId
    |> EntityId.create
    |> getSolutionsFlow
    |> Result.map (List.map fromSolution)
    |> workflowResultToHttpResponse

let getSolution (id: int) =
    let getSolutionFlow = getSolution getSolutionFromStore

    id
    |> EntityId.create
    |> getSolutionFlow
    |> Result.map fromSolution
    |> workflowResultToHttpResponse

let updateSolution (id: int) =
    authorize Admin (fun httpContext ->
        let updateSolutionFlow = updateSolution updateSolutionInStore getSolutionFromStore

        Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<SolutionFormDto>
        |> toUnvalidatedSolution
        |> updateSolutionFlow (EntityId.create id)
        |> Result.map fromSolution
        |> (fun result -> workflowResultToHttpResponse result httpContext))

let deleteSolution (id: int) =
    authorize SuperAdmin (fun httpContext ->
        let deleteSolutionFlow = deleteSolution deleteSolutionFromStore

        id
        |> EntityId.create
        |> deleteSolutionFlow
        |> Result.Ok
        |> (fun result -> workflowResultToHttpResponse result httpContext))



// User
let createUser =
    fun httpContext ->
        let createUserFlow = createUser addUserToStore getUserFromStore

        Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<UserRegistrationFormDto>
        |> toUnvalidatedUserRegistration
        |> createUserFlow
        |> Result.map fromUser
        |> (fun result -> workflowResultToHttpResponse result httpContext)

let getUsers =
    authorize Admin (fun httpContext ->
        let getUsersFlow = getUsers getUsersFromStore

        getUsersFlow ()
        |> Result.map (List.map fromUser)
        |> (fun result -> workflowResultToHttpResponse result httpContext))

let getUser (id: int) : WebPart =
    authorize User (fun httpContext ->
        let getUserFlow = getUser getUserFromStore

        id
        |> EntityId.create
        |> getUserFlow
        |> Result.map fromUser
        |> (fun result -> workflowResultToHttpResponse result httpContext))


let deleteUser (id: int) =
    authorize SuperAdmin (fun httpContext ->
        let deleteUserFlow = deleteUser deleteUserFromStore

        id
        |> EntityId.create
        |> deleteUserFlow
        |> (fun result -> workflowResultToHttpResponse result httpContext))

let login =
    fun httpContext ->

        let loginFlow = login getUserFromStoreByEmail

        Encoding.UTF8.GetString httpContext.request.rawForm
        |> JsonConvert.DeserializeObject<UserLoginFormDto>
        |> toUnvalidatedUserLogin
        |> loginFlow
        |> (fun result -> workflowResultToHttpResponse result httpContext)
