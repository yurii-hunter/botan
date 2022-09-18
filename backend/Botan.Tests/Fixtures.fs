module Botan.Tests.Fixtures

open System
open Ductus.FluentDocker.Builders
open MongoDB.Driver
open Xunit

type MongoFixture() =

    let mongoContainer =
        Builder()
            .UseContainer()
            .UseImage("mongo:latest")
            .ExposePort(27000, 27017)
            .Build()

    do mongoContainer.Start() |> ignore

    member this.Connection =
        let client = MongoClient("mongodb://localhost:27000")
        client.GetDatabase "botan-test"

    interface IDisposable with
        member this.Dispose() =
            mongoContainer.Stop()
            mongoContainer.Remove()


[<CollectionDefinition("Effects Collection")>]
type DemoCollection() =
    interface ICollectionFixture<MongoFixture>
