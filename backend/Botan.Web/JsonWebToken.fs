module Botan.Web.JsonWebToken

open System
open Jose
open Newtonsoft.Json
open System.Security.Cryptography

type UserRights =
    { Email: string
      Rights: string list
      Expires: DateTime }

let private createPassPhrase () =
    let crypto = RandomNumberGenerator.Create()
    let randomNumber = Array.init 32 byte
    crypto.GetBytes(randomNumber)
    randomNumber

let private passPhrase =
    Effects.Config.passPhrase
    |> Convert.FromBase64String

let private encodeString (payload: string) =
    JWT.Encode(payload, passPhrase, JweAlgorithm.A256KW, JweEncryption.A256CBC_HS512)

let private decodeString (jwt: string) =
    JWT.Decode(jwt, passPhrase, JweAlgorithm.A256KW, JweEncryption.A256CBC_HS512)

let encode token =
    JsonConvert.SerializeObject token |> encodeString

let decode<'a> (jwt: string) : 'a =
    decodeString jwt
    |> JsonConvert.DeserializeObject<'a>

/// Returns true if the JSON Web Token is successfully decoded and the signature is verified.
let isValid (jwt: string) : UserRights option =
    try
        let token = decode jwt
        Some token
    with _ ->
        None
