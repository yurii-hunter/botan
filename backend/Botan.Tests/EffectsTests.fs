module Botan.Tests.EffectsTests

open System
open Botan.Tests.Fixtures
open Botan.Web.Domain
open Botan.Web.Extensions
open Botan.Web.Effects
open Xunit
open Swensen.Unquote

[<Collection("Effects Collection")>]
type UniversityTests(mongo: MongoFixture) =

    do mongo.Connection.DropCollection "Universities"

    [<Fact>]
    let ``should add university to store`` () =
        // Arrange
        let validatedUniversity = { ValidatedUniversity.Name = UniversityName "KPI" }

        // Act
        let addedUniversity = addUniversityToStore mongo.Connection validatedUniversity

        // Assert
        test <@ addedUniversity.Name = (UniversityName "KPI") @>
        test <@ (EntityId.value addedUniversity.Id) <> "" @>

    [<Fact>]
    let ``should get university from store by id`` () =
        // Arrange
        let validatedUniversity = { ValidatedUniversity.Name = UniversityName "KPI" }

        // Act
        let addedUniversity = addUniversityToStore mongo.Connection validatedUniversity

        let university = getUniversityById mongo.Connection addedUniversity.Id

        // Assert
        test <@ university = Ok addedUniversity @>

[<Collection("Effects Collection")>]
type CategoryTests(mongo: MongoFixture) =

    do mongo.Connection.DropCollection "Categories"

    [<Fact>]
    let ``should add category to store`` () =
        // Arrange
        let validatedCategory = { ValidatedCategory.Name = CategoryName "Computer Science" }

        // Act
        let addedCategory = addCategoryToStore mongo.Connection validatedCategory

        // Assert
        test <@ addedCategory.Name = (CategoryName "Computer Science") @>
        test <@ (EntityId.value addedCategory.Id) <> "" @>

    [<Fact>]
    let ``should get category from store by id`` () =
        // Arrange
        let validatedCategory = { ValidatedCategory.Name = CategoryName "Computer Science" }

        // Act
        let addedCategory = addCategoryToStore mongo.Connection validatedCategory

        let category = getCategoryById mongo.Connection addedCategory.Id

        // Assert
        test <@ category = Ok addedCategory @>

[<Collection("Effects Collection")>]
type QuestionTests(mongo: MongoFixture) =

    do
        mongo.Connection.DropCollection "Categories"
        mongo.Connection.DropCollection "Universities"
        mongo.Connection.DropCollection "Questions"

    let addUniversityToStore =
        { ValidatedUniversity.Name = UniversityName "KPI" }
        |> addUniversityToStore mongo.Connection

    let addCategoryToStore =
        { ValidatedCategory.Name = CategoryName "Computer Science" }
        |> addCategoryToStore mongo.Connection

    [<Fact>]
    let ``should add question to store`` () =
        // Arrange
        let addedUniversity = addUniversityToStore
        let addedCategory = addCategoryToStore

        // Act
        let validatedQuestion =
            { Title = Title "Title"
              University = addedUniversity.Id
              Category = addedCategory.Id
              Description = Description "Description"
              Created = DateTime.Now }

        let addedQuestion =
            addQuestionToStore mongo.Connection validatedQuestion
            |> Result.getOk

        // Assert
        test <@ addedQuestion.Title = (Title "Title") @>
        test <@ addedQuestion.University = addedUniversity @>
        test <@ addedQuestion.Category = addedCategory @>
        test <@ addedQuestion.Description = (Description "Description") @>

    [<Fact>]
    let ``should get question from store by id`` () =
        // Arrange
        let addedUniversity = addUniversityToStore
        let addedCategory = addCategoryToStore

        let validatedQuestion =
            { Title = Title "Title"
              University = addedUniversity.Id
              Category = addedCategory.Id
              Description = Description "Description"
              Created = DateTime.Now }

        // Act
        let addedQuestion =
            addQuestionToStore mongo.Connection validatedQuestion
            |> Result.getOk

        let question =
            getQuestionFromStore mongo.Connection addedQuestion.Id
            |> Result.getOk

        // Assert
        test <@ question = addedQuestion @>

[<Collection("Effects Collection")>]
type AnswerTests(mongo: MongoFixture) =

    do
        mongo.Connection.DropCollection "Categories"
        mongo.Connection.DropCollection "Universities"
        mongo.Connection.DropCollection "Questions"
        mongo.Connection.DropCollection "Answers"

    let addUniversityToStore =
        { ValidatedUniversity.Name = UniversityName "KPI" }
        |> addUniversityToStore mongo.Connection

    let addCategoryToStore =
        { ValidatedCategory.Name = CategoryName "Computer Science" }
        |> addCategoryToStore mongo.Connection

    let addQuestionToStore =
        { Title = Title "Title"
          University = addUniversityToStore.Id
          Category = addCategoryToStore.Id
          Description = Description "Description"
          Created = DateTime.Now }
        |> addQuestionToStore mongo.Connection
        |> Result.getOk

    [<Fact>]
    let ``should add answer to store`` () =
        // Arrange
        let addedQuestion = addQuestionToStore

        // Act
        let validatedAnswer =
            { Code = Code "Code"
              Language = Language.Pascal }

        let answer =
            addAnswerToStore mongo.Connection addedQuestion.Id validatedAnswer
            |> Result.getOk

        // Assert
        test <@ answer.Code = (Code "Code") @>
        test <@ answer.Language = Language.Pascal @>
        test <@ answer.Id <> EntityId "" @>

[<Collection("Effects Collection")>]
type FilterTests(mongo: MongoFixture) =

    do
        mongo.Connection.DropCollection "Categories"
        mongo.Connection.DropCollection "Universities"
        mongo.Connection.DropCollection "Questions"
        mongo.Connection.DropCollection "Answers"

    let addUniversityToStore =
        { ValidatedUniversity.Name = UniversityName "KPI" }
        |> addUniversityToStore mongo.Connection

    let addCategoryToStore =
        { ValidatedCategory.Name = CategoryName "Computer Science" }
        |> addCategoryToStore mongo.Connection

    let addQuestionToStore =
        { Title = Title "Title"
          University = addUniversityToStore.Id
          Category = addCategoryToStore.Id
          Description = Description "Description"
          Created = DateTime.Now }
        |> addQuestionToStore mongo.Connection
        |> Result.getOk

    [<Fact>]
    let ``should get all question from store when filter is not specified`` () =
        // Arrange
        let answeredQuestion = addQuestionToStore
        let unansweredQuestion = addQuestionToStore

        { Code = Code "Code"
          Language = Language.Pascal }
        |> addAnswerToStore mongo.Connection answeredQuestion.Id
        |> ignore

        // Act
        let validatedQuery =
            { Answered = None
              Languages = List.empty }

        let questionsIds =
            getAllQuestionsFromStore mongo.Connection validatedQuery
            |> Result.getOk
            |> Seq.map (fun x -> x.Id)

        // Assert
        <@
            questionsIds = [ answeredQuestion.Id
                             unansweredQuestion.Id ]
        @>

    [<Fact>]
    let ``should get all answered questions from store when filter is Answered`` () =
        // Arrange
        let answeredQuestion = addQuestionToStore
        let _ = addQuestionToStore

        { Code = Code "Code"
          Language = Language.Pascal }
        |> addAnswerToStore mongo.Connection answeredQuestion.Id
        |> ignore

        // Act
        let validatedQuery =
            { Answered = Some true
              Languages = List.empty }

        let questionsIds =
            getAllQuestionsFromStore mongo.Connection validatedQuery
            |> Result.getOk
            |> Seq.map (fun x -> x.Id)

        // Assert
        <@ questionsIds = [ answeredQuestion.Id ] @>

    [<Fact>]
    let ``should get all answered questions by specific lang from store when lang filter specified`` () =
        // Arrange
        let pascalQuestion = addQuestionToStore
        let csharpQuestion = addQuestionToStore

        { Code = Code "Code"
          Language = Language.Pascal }
        |> addAnswerToStore mongo.Connection pascalQuestion.Id
        |> ignore

        { Code = Code "Code"
          Language = Language.CSharp }
        |> addAnswerToStore mongo.Connection csharpQuestion.Id
        |> ignore

        // Act
        let validatedQuery =
            { Answered = Some true
              Languages = [ Language.CSharp ] }

        let questionsIds =
            getAllQuestionsFromStore mongo.Connection validatedQuery
            |> Result.getOk
            |> Seq.map (fun x -> x.Id)

        // Assert
        <@ questionsIds = [ csharpQuestion.Id ] @>
