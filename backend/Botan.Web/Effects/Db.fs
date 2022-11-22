namespace Botan.Web.Effects

open System.Data
open System.Reflection
open Botan.Web.Domain.Errors
open DbUp
open Npgsql

module Db =

    let migrate () =

        EnsureDatabase.For.PostgresqlDatabase(Config.dbConnectionString)

        let upgradeEngine =
            DeployChanges
                .To
                .PostgresqlDatabase(Config.dbConnectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build()

        let upgradeResult = upgradeEngine.PerformUpgrade()

        if upgradeResult.Successful then
            Result.Ok "Database migration successful"
        else
            Result.Error upgradeResult.Error

    let conn () =
        new NpgsqlConnection(Config.dbConnectionString)

    let newCommand sql conn = new NpgsqlCommand(sql, conn)

    let setParam (name: string) value (command: NpgsqlCommand) =
        command.Parameters.AddWithValue(name, value)
        |> ignore

        command

    let query (command: NpgsqlCommand) =
        try
            command.Connection.Open()

            try
                let da = new NpgsqlDataAdapter(command)
                let ds = new DataSet()
                da.Fill(ds) |> ignore

                ds.Tables[0].Rows
                |> Seq.cast<DataRow>
                |> List.ofSeq
                |> Result.Ok
            with ex ->
                AppError.createResult (DataBaseError ex)
        finally
            command.Connection.Dispose()

    let querySingleOrDefault (command: NpgsqlCommand) =
        query command
        |> Result.map (fun rows -> rows |> Seq.tryHead)

    let execScalar (command: NpgsqlCommand) =
        try
            command.Connection.Open()

            try
                command.ExecuteScalar() :?> int |> Result.Ok
            with
            | :? PostgresException as ex when ex.SqlState = "23505" -> AppError.createResult (DuplicateRecord ex)
            | ex -> AppError.createResult (DataBaseError ex)
        finally
            command.Connection.Dispose()

    let exec (command: NpgsqlCommand) =
        try
            command.Connection.Open()

            try
                command.ExecuteNonQuery() |> ignore |> Result.Ok
            with ex ->
                AppError.createResult (DataBaseError ex)
        finally
            command.Connection.Dispose()
