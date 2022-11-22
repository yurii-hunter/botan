namespace Botan.Web.Effects

open System.Data
open Botan.Web.Domain
open Botan.Web.Domain.Errors
open Botan.Web.Effects
open Npgsql

module UniversityStore =
    let private toUniversity (university: DataRow) : University =
        { Id = EntityId(university["id"] :?> int)
          Name = UniversityName(string university["name"]) }

    let addUniversityToStore (validatedUniversity: ValidatedUniversity) =
        let sql = "INSERT INTO universities (name) VALUES (@name) RETURNING id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "name" (UniversityName.value validatedUniversity.Name)
        |> Db.execScalar
        |> Result.map EntityId

    let getUniversityFromStore universityId =
        let sql = "SELECT * FROM universities WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value universityId)
        |> Db.querySingleOrDefault
        |> Result.bind (Result.ofOption (AppError.create (RecordNotFound("university", EntityId.value universityId))))
        |> Result.map toUniversity

    let getUniversitiesFromStore () =
        let sql = "SELECT * FROM universities"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.query
        |> Result.map (List.map toUniversity)

    let updateUniversityInStore (id: EntityId) (validatedUniversity: ValidatedUniversity) =
        let sql = "UPDATE universities SET name = @name WHERE id = @id RETURNING id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "name" (UniversityName.value validatedUniversity.Name)
        |> Db.setParam "id" (EntityId.value id)
        |> Db.exec

    let deleteUniversityFromStore (id: EntityId) =
        let sql = "DELETE FROM universities WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value id)
        |> Db.exec
