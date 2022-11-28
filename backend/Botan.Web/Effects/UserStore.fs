namespace Botan.Web.Effects

open System.Data
open Botan.Web.Domain
open Botan.Web.Extensions
open Botan.Web.Extensions.Expr
open Botan.Web.Domain.Errors

module UserStore =

    let private toUser (user: DataRow) =
        result {
            let! userRole = UserRole.create (string user["role"])

            return
                { Id = EntityId(user["id"] :?> int)
                  Name = UserName(string user["name"])
                  Email = Email(string user["email"])
                  HashedPassword = HashedPassword(string user["password"])
                  Role = userRole }
        }

    let addUserToStore (validatedUser: UserRegistrationWithHashedPassword) =
        let sql =
            "INSERT INTO users (name, email, password, role) VALUES (@name, @email, @password, @role) RETURNING id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "name" (UserName.value validatedUser.Name)
        |> Db.setParam "email" (Email.value validatedUser.Email)
        |> Db.setParam "password" (HashedPassword.value validatedUser.HashedPassword)
        |> Db.setParam "role" (UserRole.toString validatedUser.Role)
        |> Db.execScalar
        |> Result.map EntityId

    let getUserFromStore (userId: EntityId) =
        let sql = "SELECT * FROM users WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value userId)
        |> Db.querySingleOrDefault
        |> Result.bind (Result.ofOption (AppError.create (RecordNotFound("user", EntityId.value userId))))
        |> Result.bind toUser

    let getUsersFromStore () =
        let sql = "SELECT * FROM users"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.query
        |> Result.bind (List.map toUser >> Result.sequence)

    let deleteUserFromStore (id: EntityId) =
        let sql = "DELETE FROM users WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value id)
        |> Db.exec

    let getUserFromStoreByEmail (email: Email) =
        let sql = "SELECT * FROM users WHERE email = @email"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "email" (Email.value email)
        |> Db.querySingleOrDefault
        |> Result.bind (Result.ofOption (AppError.create (RecordNotFound("user", Email.value email))))
        |> Result.bind toUser
