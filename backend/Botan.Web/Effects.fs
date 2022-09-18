module Botan.Web.Effects

open Botan.Web.Domain
open Botan.Web.Extensions
open MongoDB.Bson
open MongoDB.Driver

// Converters
let private toUniversity (university: BsonValue) =
    { University.Id = EntityId(string university["_id"])
      Name = UniversityName(university["Name"].AsString) }

let private toCategory (category: BsonValue) =
    { Category.Id = EntityId(string category["_id"])
      Name = CategoryName(category["Name"].AsString) }

let private toAnswer (answer: BsonValue) =
    Language.create (answer["Language"].AsString)
    |> Result.map (fun lang ->
        { Id = EntityId(string answer["_id"])
          Code = Code(answer["Code"].AsString)
          Language = lang })

let private toQuestion (question: BsonValue) =
    question["Answers"].AsBsonArray
    |> Seq.map toAnswer
    |> List.ofSeq
    |> Result.sequence
    |> Result.map (fun answers ->
        { Id = EntityId(string question["_id"])
          Title = Title question["Title"].AsString
          University = toUniversity question["University"]
          Category = toCategory question["Category"]
          Description = Description question["Description"].AsString
          Created = question[ "Created" ].ToUniversalTime()
          Answers = answers })

let private toMongoFilter validatedQuery =
    let builder = Builders<BsonDocument>.Filter

    let answeredFilter =
        match validatedQuery.Answered with
        | Some answered when answered = true -> builder.SizeGt("Answers", 0)
        | Some answered when answered = false -> builder.Size("Answers", 0)
        | _ -> builder.Empty

    let langFilter =
        match validatedQuery.Languages with
        | [] -> builder.Empty
        | languages ->
            let languagesDoc =
                languages
                |> List.map (fun language -> BsonValue.Create(Language.value language))

            builder.In("Answers.Language", languagesDoc)

    builder.And [| answeredFilter; langFilter |]

// University
let addUniversityToStore (dbClient: IMongoDatabase) (validatedUniversity: ValidatedUniversity) =
    let university =
        BsonDocument([ BsonElement("Name", BsonValue.Create(UniversityName.value validatedUniversity.Name)) ])

    let collection = dbClient.GetCollection<BsonDocument> "Universities"

    collection.InsertOne(university)

    { University.Id = EntityId(string university["_id"])
      Name = UniversityName university["Name"].AsString }

let getUniversityById (dbClient: IMongoDatabase) (universityId: EntityId) =
    let collection = dbClient.GetCollection<BsonDocument> "Universities"

    let university =
        collection
            .Find(BsonDocument("_id", BsonObjectId.Create(EntityId.value universityId)))
            .ToList()
        |> Seq.first

    university
    |> Option.map toUniversity
    |> Result.ofOption "University not found"

// Category
let addCategoryToStore (dbClient: IMongoDatabase) (validatedCategory: ValidatedCategory) =
    let category =
        BsonDocument([ BsonElement("Name", BsonValue.Create(CategoryName.value validatedCategory.Name)) ])

    let collection = dbClient.GetCollection<BsonDocument> "Categories"

    collection.InsertOne(category)

    toCategory category

let getCategoryById (dbClient: IMongoDatabase) (categoryId: EntityId) =
    let collection = dbClient.GetCollection<BsonDocument> "Categories"

    let category =
        collection
            .Find(BsonDocument("_id", BsonObjectId.Create(EntityId.value categoryId)))
            .ToList()
        |> Seq.first

    category
    |> Option.map toCategory
    |> Result.ofOption "Category not found"

// Question
let getQuestionFromStore (dbClient: IMongoDatabase) (questionId: EntityId) =
    let questionsCollection = dbClient.GetCollection<BsonDocument> "Questions"

    let question =
        questionsCollection
            .Aggregate()
            .Lookup("Universities", "UniversityId", "_id", "University")
            .Unwind("University")
            .Lookup("Categories", "CategoryId", "_id", "Category")
            .Unwind("Category")
            .Lookup("Answers", "_id", "QuestionId", "Answers")
            .Match(BsonDocument("_id", ObjectId(EntityId.value questionId)))
            .ToList()
        |> Seq.first

    question
    |> Option.map toQuestion
    |> Option.defaultValue (Error "Questions not found")

let getAllQuestionsFromStore (dbClient: IMongoDatabase) (validatedQuery: ValidatedQuery) =
    let questionsCollection = dbClient.GetCollection<BsonDocument> "Questions"

    let allQuestions =
        questionsCollection
            .Aggregate()
            .Lookup("Universities", "UniversityId", "_id", "University")
            .Unwind("University")
            .Lookup("Categories", "CategoryId", "_id", "Category")
            .Unwind("Category")
            .Lookup("Answers", "_id", "QuestionId", "Answers")
            .Match(toMongoFilter validatedQuery)
            .ToList()

    allQuestions
    |> Seq.map toQuestion
    |> List.ofSeq
    |> Result.sequence

let addQuestionToStore (dbClient: IMongoDatabase) (validatedQuestion: ValidatedQuestion) =
    let question =
        BsonDocument(
            [ BsonElement("Title", BsonValue.Create(Title.value validatedQuestion.Title))
              BsonElement("UniversityId", ObjectId(EntityId.value validatedQuestion.University))
              BsonElement("CategoryId", ObjectId(EntityId.value validatedQuestion.Category))
              BsonElement("Description", BsonValue.Create(Description.value validatedQuestion.Description))
              BsonElement("Created", BsonDateTime.Create(validatedQuestion.Created)) ]
        )

    let collection = dbClient.GetCollection<BsonDocument> "Questions"

    collection.InsertOne(question)
    getQuestionFromStore dbClient (EntityId(string question["_id"]))

// Answer
let addAnswerToStore (dbClient: IMongoDatabase) (questionId: EntityId) (validatedAnswer: ValidatedAnswer) =
    let answer =
        BsonDocument(
            [ BsonElement("Code", BsonValue.Create(Code.value validatedAnswer.Code))
              BsonElement("Language", BsonValue.Create(Language.value validatedAnswer.Language))
              BsonElement("QuestionId", ObjectId(EntityId.value questionId)) ]
        )

    let answersCollection = dbClient.GetCollection<BsonDocument> "Answers"

    answersCollection.InsertOne(answer)

    toAnswer answer

let getAnswersFromStore (dbClient: IMongoDatabase) (questionId: string) =
    let collection = dbClient.GetCollection<BsonDocument> "Answers"

    let allAnswers =
        collection
            .Find(BsonDocument("QuestionId", BsonString.Create questionId))
            .ToList()

    allAnswers
    |> Seq.map toAnswer
    |> List.ofSeq
    |> Result.sequence

// Db
let getDBClient () =
    let client = MongoClient("mongodb://localhost:27017")

    client.GetDatabase("botan")
