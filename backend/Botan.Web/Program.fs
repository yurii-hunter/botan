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
                  [ path "/api/universities" >=> createUniversity
                    pathScan "/api/universities/%d/courses" createCourse
                    pathScan "/api/courses/%d/tasks" createTask
                    pathScan "/api/tasks/%d/solutions" createSolution
                    path "/api/users" >=> createUser ]

          GET
          >=> choose
                  [ path "/api/universities" >=> getUniversities
                    pathScan "/api/universities/%d" getUniversity
                    pathScan "/api/universities/%d/courses" getCourses
                    pathScan "/api/courses/%d" getCourse
                    pathScan "/api/courses/%d/tasks" getTasks
                    pathScan "/api/tasks/%d" getTask
                    pathScan "/api/tasks/%d/solutions" getSolutions
                    pathScan "/api/solutions/%d" getSolution
                    path "/api/users" >=> getUsers
                    pathScan "/api/users/%d" getUser ]
          PUT
          >=> choose
                  [ pathScan "/api/universities/%d" updateUniversity
                    pathScan "/api/courses/%d" updateCourse
                    pathScan "/api/tasks/%d" updateTask
                    pathScan "/api/solutions/%d" updateSolution ]

          DELETE
          >=> choose
                  [ pathScan "/api/universities/%d" deleteUniversity
                    pathScan "/api/courses/%d" deleteCourse
                    pathScan "/api/tasks/%d" deleteTask
                    pathScan "/api/solutions/%d" deleteSolution
                    pathScan "/api/users/%d" deleteUser ]

          path "/"
          >=> (Successful.OK "This will return the base page.") ]

[<EntryPoint>]
let main argv =
    Effects.Db.migrate () |> ignore
    startWebServer defaultConfig (app ())
    0
