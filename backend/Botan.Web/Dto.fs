module Botan.Web.Dto

open Botan.Web.Domain
open Suave.Http
open Suave.Utils

// University
type UniversityFormDto = { Name: string }

module UniversityFormDto =
    let toUnvalidatedUniversity (universityFormDto: UniversityFormDto) =
        { UnvalidatedUniversity.Name = universityFormDto.Name }

type UniversityDto = { Id: int; Name: string }

module UniversityDto =
    let fromUniversity (university: University) =
        { Id = EntityId.value university.Id
          Name = UniversityName.value university.Name }

// Course
type CourseFormDto = { Name: string }

module CourseFormDto =
    let toUnvalidatedCourse (courseFormDto: CourseFormDto) : UnvalidatedCourse = { Name = courseFormDto.Name }

type CourseDto = { Id: int; Name: string }

module CourseDto =
    let fromCourse (course: Course) =
        { Id = EntityId.value course.Id
          Name = CourseName.value course.Name }

// Task
type TaskFormDto = { Title: string; Description: string }

module TaskFormDto =
    let toUnvalidatedTask taskFormDto : UnvalidatedTask =
        { Title = taskFormDto.Title
          Description = taskFormDto.Description }

type TaskDto =
    { Id: int
      Title: string
      Description: string }

module TaskDto =
    let fromTask (task: Task) =
        { Id = EntityId.value task.Id
          Title = Title.value task.Title
          Description = Description.value task.Description }

    let fromTasks tasks = tasks |> List.map fromTask

// Solution
type SolutionFormDto = { Code: string; Language: string }

module SolutionFormDto =
    let toUnvalidatedSolution solutionFormDto : UnvalidatedSolution =
        { Code = solutionFormDto.Code
          Language = solutionFormDto.Language }

type SolutionDto =
    { Id: int
      Code: string
      Language: string }

module SolutionDto =
    let fromSolution (solution: Solution) : SolutionDto =
        { Id = EntityId.value solution.Id
          Code = Code.value solution.Code
          Language = Language.value solution.Language }

// User
type UserRegistrationFormDto =
    { Name: string
      Email: string
      Password: string
      PasswordConfirmation: string }

module UserRegistrationFormDto =
    let toUnvalidatedUserRegistration (userRegistrationFormDto: UserRegistrationFormDto) : UnvalidatedUserRegistration =
        { Email = userRegistrationFormDto.Email
          Password = userRegistrationFormDto.Password
          PasswordConfirmation = userRegistrationFormDto.PasswordConfirmation
          Name = userRegistrationFormDto.Name }

type UserLoginFormDto = { Email: string; Password: string }

module UserLoginFormDto =
    let toUnvalidatedUserLogin (userLoginFormDto: UserLoginFormDto) : UnvalidatedUserLogin =
        { Email = userLoginFormDto.Email
          Password = userLoginFormDto.Password }

type UserDto =
    { Id: int; Name: string; Email: string }

module UserDto =
    let fromUser (user: User) : UserDto =
        { Id = EntityId.value user.Id
          Name = UserName.value user.Name
          Email = Email.value user.Email }

// Query
module QueryDto =
    let toUnvalidatedQuery (request: HttpRequest) : UnvalidatedQuery =
        let solved =
            request.queryParam "solved"
            |> Choice.map bool.Parse
            |> Option.ofChoice

        let languages =
            request.query
            |> List.filter (fun q -> fst q = "lang")
            |> List.choose snd

        { Solved = solved
          Languages = languages }
