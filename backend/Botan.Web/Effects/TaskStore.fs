namespace Botan.Web.Effects

open System.Data
open Botan.Web.Domain
open Botan.Web.Domain.Errors

module TaskStore =
    let toTask (task: DataRow) : Task =
        { Id = EntityId(task["id"] :?> int)
          Title = Title(string task["title"])
          Description = Description(string task["description"]) }

    let addTaskToStore (courseId: EntityId) (validatedTask: ValidatedTask) =
        let sql =
            "INSERT INTO tasks (title, description, course_id) VALUES (@title, @description, @course_id) RETURNING id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "title" (Title.value validatedTask.Title)
        |> Db.setParam "description" (Description.value validatedTask.Description)
        |> Db.setParam "course_id" (EntityId.value courseId)
        |> Db.execScalar
        |> Result.map EntityId

    let getTaskFromStore (taskId: EntityId) =
        let sql = "SELECT * FROM tasks WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value taskId)
        |> Db.querySingleOrDefault
        |> Result.bind (Result.ofOption (AppError.create (RecordNotFound("task", EntityId.value taskId))))
        |> Result.map toTask

    let getTasksFromStore (courseId: EntityId) =
        let sql = "SELECT * FROM tasks WHERE course_id = @course_id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "course_id" (EntityId.value courseId)
        |> Db.query
        |> Result.map (List.map toTask)

    let updateTaskInStore (id: EntityId) (validatedTask: ValidatedTask) =
        let sql =
            "UPDATE tasks SET title = @title, description = @description WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "title" (Title.value validatedTask.Title)
        |> Db.setParam "description" (Description.value validatedTask.Description)
        |> Db.setParam "id" (EntityId.value id)
        |> Db.exec

    let deleteTaskFromStore (id: EntityId) =
        let sql = "DELETE FROM tasks WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value id)
        |> Db.exec
