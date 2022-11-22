module Botan.Tests.EffectsTests

open System.Net.Http
open Botan.Tests.Fixtures
open Botan.Web.Dto
open Newtonsoft.Json
open Xunit
open Swensen.Unquote

[<Collection("Integration Tests Collection")>]
type IntegrationTests(app: AppFixture) =

    [<Fact>]
    let ``should create university`` () =
        task {
            // Arrange
            let formDto = { UniversityFormDto.Name = "University of Test" }
            let json = JsonConvert.SerializeObject formDto

            let requestBody =
                new StringContent(json, System.Text.Encoding.UTF8, "application/json")

            // Act
            let! response = app.Client.PostAsync("/universities", requestBody)
            let! responseBody = response.Content.ReadAsStringAsync()
            let actual = JsonConvert.DeserializeObject<UniversityDto>(responseBody)

            // Assert
            test
                <@
                    actual = { UniversityDto.Id = 2
                               Name = "University of Test" }
                @>
        }

    [<Fact>]
    let ``should create course`` () =
        task {
            // Arrange
            let formDto = { CourseFormDto.Name = "Test Course" }
            let json = JsonConvert.SerializeObject formDto

            let requestBody =
                new StringContent(json, System.Text.Encoding.UTF8, "application/json")

            // Act
            let! response = app.Client.PostAsync("/universities/1/courses", requestBody)
            let! responseBody = response.Content.ReadAsStringAsync()
            let actual = JsonConvert.DeserializeObject<CourseDto>(responseBody)

            // Assert
            test
                <@
                    actual = { CourseDto.Id = 2
                               Name = "Test Course" }
                @>
        }

    [<Fact>]
    let ``should create task`` () =
        task {
            // Arrange
            let formDto =
                { TaskFormDto.Title = "My Test Task"
                  Description = "Test Description" }

            let json = JsonConvert.SerializeObject formDto

            let requestBody =
                new StringContent(json, System.Text.Encoding.UTF8, "application/json")

            // Act
            let! response = app.Client.PostAsync("/courses/1/tasks", requestBody)
            let! responseBody = response.Content.ReadAsStringAsync()
            let actual = JsonConvert.DeserializeObject<TaskDto>(responseBody)

            // Assert
            test
                <@
                    actual = { TaskDto.Id = 2
                               Title = "My Test Task"
                               Description = "Test Description" }
                @>
        }

    [<Fact>]
    let ``should add solution`` () =
        task {
            // Arrange
            let formDto =
                { SolutionFormDto.Code = "Test Code"
                  Language = "JavaScript" }

            let json = JsonConvert.SerializeObject formDto

            let requestBody =
                new StringContent(json, System.Text.Encoding.UTF8, "application/json")

            // Act
            let! response = app.Client.PostAsync("/tasks/1/solutions", requestBody)
            let! responseBody = response.Content.ReadAsStringAsync()
            let actual = JsonConvert.DeserializeObject<SolutionDto>(responseBody)

            // Assert
            test
                <@
                    actual = { SolutionDto.Id = 2
                               Code = "Test Code"
                               Language = "JavaScript" }
                @>
        }
