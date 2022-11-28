module Botan.Web.Main

open Botan.Web
open Suave
open Suave.Operators
open Suave.Filters
open Botan.Web.Api

let app () =
    choose
        [ POST
          >=> choose
                  [ path "/api/v1/universities" >=> createUniversity
                    pathScan "/api/v1/universities/%d/courses" createCourse
                    pathScan "/api/v1/courses/%d/tasks" createTask
                    pathScan "/api/v1/tasks/%d/solutions" createSolution
                    path "/api/v1/users" >=> createUser
                    path "/login" >=> login ]

          GET
          >=> choose
                  [ path "/api/v1/universities" >=> getUniversities
                    pathScan "/api/v1/universities/%d" getUniversity
                    pathScan "/api/v1/universities/%d/courses" getCourses
                    pathScan "/api/v1/courses/%d" getCourse
                    pathScan "/api/v1/courses/%d/tasks" getTasks
                    pathScan "/api/v1/tasks/%d" getTask
                    pathScan "/api/v1/tasks/%d/solutions" getSolutions
                    pathScan "/api/v1/solutions/%d" getSolution
                    path "/api/v1/users" >=> getUsers
                    pathScan "/api/v1/users/%d" getUser ]
          PUT
          >=> choose
                  [ pathScan "/api/v1/universities/%d" updateUniversity
                    pathScan "/api/v1/courses/%d" updateCourse
                    pathScan "/api/v1/tasks/%d" updateTask
                    pathScan "/api/v1/solutions/%d" updateSolution ]

          DELETE
          >=> choose
                  [ pathScan "/api/v1/universities/%d" deleteUniversity
                    pathScan "/api/v1/courses/%d" deleteCourse
                    pathScan "/api/v1/tasks/%d" deleteTask
                    pathScan "/api/v1/solutions/%d" deleteSolution
                    pathScan "/api/v1/users/%d" deleteUser ]

          path "/"
          >=> (Successful.OK "This will return the base page.")
          RequestErrors.NOT_FOUND "Found no handlers" ]

[<EntryPoint>]
let main argv =
    Effects.Db.migrate () |> ignore
    startWebServer defaultConfig (app ())
    0
