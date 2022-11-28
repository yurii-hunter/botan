module Botan.Web.Auth

open Botan.Web.Domain
open Suave
open Suave.RequestErrors

let authorize (role: UserRole) (protectedPart: WebPart) : WebPart =
    fun (ctx: HttpContext) ->

        let userRights =
            ctx.request.header "authorization"
            |> Option.ofChoice
            |> Option.map (fun x -> x.Replace("Bearer ", ""))
            |> Option.map (fun x -> x.Trim())
            |> Option.bind JsonWebToken.isValid


        match userRights with
        | Some user when user.Role >= role ->
            ctx.userState.Add("email", user.Email)
            ctx.userState.Add("role", user.Role)
            protectedPart ctx
        | _ -> ctx |> UNAUTHORIZED "Unauthorized"
