module d

open Suave
open Suave.Operators
open Suave.Filters
open Botan.Web.Api

let filter: WebPart =
    RequestErrors.FORBIDDEN "You are not allowed to access this resource"

let app () =
    choose
        [ POST
          >=> choose
                  [ path "/questions" >=> filter >=> createQuestion
                    path "/universities" >=> createUniversity
                    path "/categories" >=> createCategory
                    pathScan "/questions/%s/answers" addAnswer ]
          GET
          >=> choose
                  [ pathScan "/questions/%s" getQuestionById
                    path "/questions" >=> getAllQuestions ]
          path "/"
          >=> (Successful.OK "This will return the base page.") ]

[<EntryPoint>]
let main argv =
    startWebServer defaultConfig (app ())
    0
