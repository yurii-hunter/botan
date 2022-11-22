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
                  [ path "/universities" >=> createUniversity
                    pathScan "/universities/%d/courses" createCourse
                    pathScan "/courses/%d/tasks" createTask
                    pathScan "/tasks/%d/solutions" createSolution
                    path "/users" >=> createUser ]

          GET
          >=> choose
                  [ path "/universities" >=> getUniversities
                    pathScan "/universities/%d" getUniversity
                    pathScan "/universities/%d/courses" getCourses
                    pathScan "/courses/%d" getCourse
                    pathScan "/courses/%d/tasks" getTasks
                    pathScan "/tasks/%d" getTask
                    pathScan "/tasks/%d/solutions" getSolutions
                    pathScan "/solutions/%d" getSolution
                    path "/users" >=> getUsers
                    pathScan "/users/%d" getUser ]
          PUT
          >=> choose
                  [ pathScan "/universities/%d" updateUniversity
                    pathScan "/courses/%d" updateCourse
                    pathScan "/tasks/%d" updateTask
                    pathScan "/solutions/%d" updateSolution ]

          DELETE
          >=> choose
                  [ pathScan "/universities/%d" deleteUniversity
                    pathScan "/courses/%d" deleteCourse
                    pathScan "/tasks/%d" deleteTask
                    pathScan "/solutions/%d" deleteSolution
                    pathScan "/users/%d" deleteUser ]

          path "/"
          >=> (Successful.OK "This will return the base page.") ]

[<EntryPoint>]
let main argv =
    Effects.Db.migrate () |> ignore
    startWebServer defaultConfig (app ())
    0
