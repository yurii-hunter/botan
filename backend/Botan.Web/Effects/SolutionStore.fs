namespace Botan.Web.Effects

open System.Data
open Botan.Web.Domain
open Botan.Web.Domain.Errors
open Botan.Web.Extensions

module SolutionStore =
    let private toSolution (solution: DataRow) =
        Language.create (string solution["language"])
        |> Result.map (fun lang ->
            { Id = EntityId(solution["id"] :?> int)
              Code = Code(string solution["code"])
              Language = lang })

    let addSolutionToStore (taskId: EntityId) (validatedSolution: ValidatedSolution) =
        let sql =
            "INSERT INTO solutions (code, language, task_id) VALUES (@code, @language, @task_id) RETURNING id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "code" (Code.value validatedSolution.Code)
        |> Db.setParam "language" (Language.toString validatedSolution.Language)
        |> Db.setParam "task_id" (EntityId.value taskId)
        |> Db.execScalar
        |> Result.map EntityId

    let getSolutionFromStore (solutionId: EntityId) =
        let sql = "SELECT * FROM solutions WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value solutionId)
        |> Db.querySingleOrDefault
        |> Result.bind (Result.ofOption (AppError.create (RecordNotFound("solution", EntityId.value solutionId))))
        |> Result.bind toSolution

    let getSolutionsFromStore (taskId: EntityId) =
        let sql = "SELECT * FROM solutions WHERE task_id = @task_id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "task_id" (EntityId.value taskId)
        |> Db.query
        |> Result.bind (List.map toSolution >> Result.sequence)

    let updateSolutionInStore (id: EntityId) (validatedSolution: ValidatedSolution) =
        let sql = "UPDATE solutions SET code = @code, language = @language WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "code" (Code.value validatedSolution.Code)
        |> Db.setParam "language" (Language.toString validatedSolution.Language)
        |> Db.setParam "id" (EntityId.value id)
        |> Db.exec

    let deleteSolutionFromStore (id: EntityId) =
        let sql = "DELETE FROM solutions WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value id)
        |> Db.exec
