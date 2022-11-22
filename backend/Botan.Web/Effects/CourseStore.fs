namespace Botan.Web.Effects

open System.Data
open Botan.Web.Domain
open Botan.Web.Domain.Errors

module CourseStore =
    let private toCourse (course: DataRow) : Course =
        { Id = EntityId(course["id"] :?> int)
          Name = CourseName(string course["name"]) }

    let addCourseToStore (universityId: EntityId) (validatedCourse: ValidatedCourse) =
        let sql =
            "INSERT INTO courses (name, university_id) VALUES (@name, @university_id) RETURNING id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "name" (CourseName.value validatedCourse.Name)
        |> Db.setParam "university_id" (EntityId.value universityId)
        |> Db.execScalar
        |> Result.map EntityId

    let getCourseFromStore (courseId: EntityId) =
        let sql = "SELECT * FROM courses WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value courseId)
        |> Db.querySingleOrDefault
        |> Result.bind (Result.ofOption (AppError.create (RecordNotFound("course", EntityId.value courseId))))
        |> Result.map toCourse

    let getCoursesFromStore (universityId: EntityId) =
        let sql = "SELECT * FROM courses WHERE university_id = @university_id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "university_id" (EntityId.value universityId)
        |> Db.query
        |> Result.map (List.map toCourse)

    let updateCourseInStore (id: EntityId) (validatedCourse: ValidatedCourse) =
        let sql = "UPDATE courses SET name = @name WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "name" (CourseName.value validatedCourse.Name)
        |> Db.setParam "id" (EntityId.value id)
        |> Db.exec

    let deleteCourseFromStore (id: EntityId) =
        let sql = "DELETE FROM courses WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value id)
        |> Db.exec
