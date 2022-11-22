namespace Botan.Web.Effects

module Config =

    let passPhrase = System.Environment.GetEnvironmentVariable "JWT_PASS_PHRASE"

    let dbConnectionString =
        System.Environment.GetEnvironmentVariable "POSTGRES_CONNECTION_STRING"
