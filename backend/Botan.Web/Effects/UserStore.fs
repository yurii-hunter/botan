namespace Botan.Web.Effects

open System.Data
open Botan.Web.Domain
open Botan.Web.Domain.Errors

module UserStore =
    let private toUser (user: DataRow) =
        { Id = EntityId(user["id"] :?> int)
          Name = UserName(string user["name"])
          Email = Email(string user["email"])
          HashedPassword = HashedPassword(string user["password"]) }

    let addUserToStore (validatedUser: UserRegistrationWithHashedPassword) =
        let sql =
            "INSERT INTO users (name, email, password) VALUES (@name, @email, @password) RETURNING id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "name" (UserName.value validatedUser.Name)
        |> Db.setParam "email" (Email.value validatedUser.Email)
        |> Db.setParam "password" (HashedPassword.value validatedUser.HashedPassword)
        |> Db.execScalar
        |> Result.map EntityId

    let getUserFromStore (userId: EntityId) =
        let sql = "SELECT * FROM users WHERE id = @id"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.setParam "id" (EntityId.value userId)
        |> Db.querySingleOrDefault
        |> Result.bind (Result.ofOption (AppError.create (RecordNotFound("user", EntityId.value userId))))
        |> Result.map toUser

    let getUsersFromStore () =
        let sql = "SELECT * FROM users"

        Db.conn ()
        |> Db.newCommand sql
        |> Db.query
        |> Result.map (List.map toUser)

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
        |> Result.map toUser
