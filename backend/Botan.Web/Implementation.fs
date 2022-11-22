module Botan.Web.Implementation

open System
open Botan.Web.Domain
open Botan.Web.Domain.Errors
open Botan.Web.Hash
open FSharp.Core
open Botan.Web.Extensions.Result

// validation
let validateQuery (unvalidatedQuery: UnvalidatedQuery) =
    result {
        let solved = unvalidatedQuery.Solved

        let! languages =
            unvalidatedQuery.Languages
            |> List.map Language.create
            |> sequence

        return
            { ValidatedQuery.Solved = solved
              Languages = languages }
    }

// University
let validateUniversity (unvalidatedUniversity: UnvalidatedUniversity) =
    UniversityName.create unvalidatedUniversity.Name
    |> Result.map (fun name -> { ValidatedUniversity.Name = name })


let createUniversity addUniversityToStore getUniversityFromStore unvalidatedUniversity =
    unvalidatedUniversity
    |> validateUniversity
    |> Result.bind addUniversityToStore
    |> Result.bind getUniversityFromStore

let getUniversity getUniversityFromStore (universityId: EntityId) = universityId |> getUniversityFromStore

let getUniversities getUniversitiesFromStore = getUniversitiesFromStore

let updateUniversity updateUniversityInStore getUniversityFromStore (universityId: EntityId) unvalidatedUniversity =
    unvalidatedUniversity
    |> validateUniversity
    |> Result.bind (updateUniversityInStore universityId)
    |> Result.bind (fun _ -> getUniversityFromStore universityId)

let deleteUniversity deleteUniversityFromStore (universityId: EntityId) = deleteUniversityFromStore universityId

// Course
let validateCourse (unvalidatedCourse: UnvalidatedCourse) =
    CourseName.create unvalidatedCourse.Name
    |> Result.map (fun name -> { ValidatedCourse.Name = name })

let createCourse addCourseToStore getCourseFromStore universityId unvalidatedCourse =
    unvalidatedCourse
    |> validateCourse
    |> Result.bind (addCourseToStore universityId)
    |> Result.bind getCourseFromStore

let getCourse getCourseFromStore (categoryId: EntityId) = categoryId |> getCourseFromStore

let getCourses getCoursesFromStore (universityId: EntityId) = universityId |> getCoursesFromStore

let updateCourse updateCourseInStore getCourseFromStore (courseId: EntityId) unvalidatedCourse =
    unvalidatedCourse
    |> validateCourse
    |> Result.bind (updateCourseInStore courseId)
    |> Result.bind (fun _ -> getCourseFromStore courseId)

let deleteCourse deleteCourseFromStore (courseId: EntityId) = deleteCourseFromStore courseId

// Task
let validateTask (unvalidatedTask: UnvalidatedTask) =
    result {
        let! title = Title.create unvalidatedTask.Title
        let! description = Description.create unvalidatedTask.Description

        return
            { Title = title
              Description = description }
    }

let createTask addTaskToStore getTaskFromStore courseId unvalidatedTask =
    unvalidatedTask
    |> validateTask
    |> Result.bind (addTaskToStore courseId)
    |> Result.bind getTaskFromStore

let getTasks getTasksFromStore (courseId: EntityId) = courseId |> getTasksFromStore

let getTask getTaskFromStore (taskId: EntityId) = taskId |> getTaskFromStore

let updateTask updateTaskInStore getTaskFromStore (taskId: EntityId) unvalidatedTask =
    unvalidatedTask
    |> validateTask
    |> Result.bind (updateTaskInStore taskId)
    |> Result.bind (fun _ -> getTaskFromStore taskId)

let deleteTask deleteTaskFromStore (taskId: EntityId) = deleteTaskFromStore taskId

// Solution
let validateSolution (unvalidatedSolution: UnvalidatedSolution) =
    result {
        let! code = Code.create unvalidatedSolution.Code
        let! language = Language.create unvalidatedSolution.Language

        return
            { ValidatedSolution.Code = code
              Language = language }
    }

let createSolution addSolutionToStore getSolutionFromStore taskId unvalidatedSolution =
    unvalidatedSolution
    |> validateSolution
    |> Result.bind (addSolutionToStore taskId)
    |> Result.bind getSolutionFromStore

let getSolutions getSolutionsFromStore (taskId: EntityId) = taskId |> getSolutionsFromStore

let getSolution getSolutionFromStore (solutionId: EntityId) = solutionId |> getSolutionFromStore

let updateSolution updateSolutionInStore getSolutionFromStore (solutionId: EntityId) unvalidatedSolution =
    unvalidatedSolution
    |> validateSolution
    |> Result.bind (updateSolutionInStore solutionId)
    |> Result.bind (fun _ -> getSolutionFromStore solutionId)

let deleteSolution deleteSolutionFromStore (solutionId: EntityId) = deleteSolutionFromStore solutionId

// User
let validateUserRegistration (unvalidatedUser: UnvalidatedUserRegistration) =
    result {
        let! email = Email.create unvalidatedUser.Email
        let! password = Password.create unvalidatedUser.Password
        let! passwordConfirmation = Password.create unvalidatedUser.PasswordConfirmation
        let! name = UserName.create unvalidatedUser.Name

        if password <> passwordConfirmation then
            return! AppError.createResult PasswordConfirmation
        else
            return!
                Ok
                    { ValidatedUserRegistration.Email = email
                      Password = password
                      Name = name }
    }

let validateUserLogin (unvalidatedUser: UnvalidatedUserLogin) =
    result {
        let! email = Email.create unvalidatedUser.Email
        let! password = Password.create unvalidatedUser.Password

        return
            { ValidatedUserLogin.Email = email
              Password = password }
    }

let hashPassword (validatedUserRegistration: ValidatedUserRegistration) : UserRegistrationWithHashedPassword =
    validatedUserRegistration.Password
    |> Crypto.strongHash
    |> (fun hashedPassword ->
        { Name = validatedUserRegistration.Name
          Email = validatedUserRegistration.Email
          HashedPassword = HashedPassword hashedPassword })

let createUser addUserToStore getUserFromStore unvalidatedUser =
    unvalidatedUser
    |> validateUserRegistration
    |> Result.map hashPassword
    |> Result.bind addUserToStore
    |> Result.bind getUserFromStore

let getUsers getUsersFromStore = getUsersFromStore

let getUser getUserFromStore (userId: EntityId) = userId |> getUserFromStore

let deleteUser deleteUserFromStore (userId: EntityId) = deleteUserFromStore userId

let login getUserFromStore unvalidatedUser =
    result {
        let! validatedUserLogin = validateUserLogin unvalidatedUser

        let! user = getUserFromStore validatedUserLogin.Email

        if Crypto.verify user.HashedPassword validatedUserLogin.Password then
            let rights: JsonWebToken.UserRights =
                { Email = Email.value user.Email
                  Rights = [ "user" ]
                  Expires = DateTime.Now.AddMinutes 30.0 }

            let token = JsonWebToken.encode rights
            return! Ok token
        else
            return! AppError.createResult InvalidCredentials
    }
