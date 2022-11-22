module Botan.Tests.Fixtures

open System
open System.Net.Http
open System.Threading
open Botan.Web.Effects
open Ductus.FluentDocker.Builders
open Suave
open Xunit

type PostgresFixture() =

    let postgresContainer =
        Builder()
            .UseContainer()
            .UseImage("postgres:latest")
            .WithName("postgres-test")
            .WithHostName("postgres-test")
            .ExposePort(5433, 5432)
            .WithEnvironment(
                [| "POSTGRES_PASSWORD=postgres"
                   "POSTGRES_USER=postgres"
                   "POSTGRES_DB=botan" |]
            )
            .WaitForMessageInLog("listening on IPv4 address", TimeSpan.FromSeconds 10.)
            .Build()

    do postgresContainer.Start() |> ignore

    interface IDisposable with
        member this.Dispose() =
            postgresContainer.Stop()
            postgresContainer.Remove()

type AppFixture() =

    let cts = new CancellationTokenSource()
    let _, server = startWebServerAsync defaultConfig (Botan.Web.Main.app ())

    do
        Environment.SetEnvironmentVariable(
            "POSTGRES_CONNECTION_STRING",
            "Host = localhost; User Id = postgres; Password = postgres; Database = botan; Port = 5433; Timeout = 5"
        )

        Db.migrate () |> ignore
        Async.Start(server, cts.Token)

    member this.Client =
        let client = new HttpClient()
        client.BaseAddress <- Uri("http://127.0.0.1:8080")
        client

    interface IDisposable with
        member this.Dispose() = cts.Cancel()

type DataSeedFixture() =

    do
        let insertUniversitySql =
            "INSERT INTO universities (name) VALUES ('University of Oxford')"

        Db.conn ()
        |> Db.newCommand insertUniversitySql
        |> Db.exec
        |> ignore

        let insertCourseSql =
            "INSERT INTO courses (name, university_id) VALUES ('Computer Science', '1')"

        Db.conn ()
        |> Db.newCommand insertCourseSql
        |> Db.exec
        |> ignore

        let insertTaskSql =
            "INSERT INTO tasks (title, description, course_id) VALUES ('Task Title', 'Task Description', '1')"

        Db.conn ()
        |> Db.newCommand insertTaskSql
        |> Db.exec
        |> ignore

        let insertSolutionSql =
            "INSERT INTO solutions (code, language, task_id) VALUES ('Solution Code', 'javascript', '1')"

        Db.conn ()
        |> Db.newCommand insertSolutionSql
        |> Db.exec
        |> ignore

[<CollectionDefinition("Integration Tests Collection")>]
type DemoCollection() =
    interface ICollectionFixture<DataSeedFixture>
    interface ICollectionFixture<AppFixture>
    interface ICollectionFixture<PostgresFixture>
