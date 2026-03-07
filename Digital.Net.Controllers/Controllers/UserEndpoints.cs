using System.Text.Json;
using Digital.Net.Authentication.Exceptions;
using Digital.Net.Authentication.Filters;
using Digital.Net.Authentication.Services.Authentication;
using Digital.Net.Controllers.Dto;
using Digital.Net.Core.Formatters;
using Digital.Net.Core.Messages;
using Digital.Net.Core.Services.Users;
using Digital.Net.Entities.Crud;
using Digital.Net.Entities.Crud.Controllers;
using Digital.Net.Entities.Exceptions;
using Digital.Net.Entities.Models.Documents;
using Digital.Net.Entities.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Controllers.Controllers;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/user")
            .WithTags("User")
            .RequireAuthentication(AuthorizeType.Any);

        group.MapCrudSchema<User>("");

        group
            .MapGet("/self", GetSelf)
            .WithSummary("GetSelf")
            .WithDescription("Retrieves the authenticated user's information.");

        group
            .MapPatch("/self", PatchSelf)
            .WithSummary("PatchSelf")
            .WithDescription("Updates the authenticated user's information using a JSON patch. Use the *User Schema* to get the available fields.");

        group
            .MapPut("/self/password", UpdatePassword)
            .WithSummary("UpdatePassword")
            .WithDescription("Updates the authenticated user's password.");

        group
            .MapPut("/self/avatar", UpdateAvatar)
            .WithSummary("UpdateAvatar")
            .WithDescription("Updates the authenticated user's avatar.");

        group
            .MapDelete("/self/avatar", RemoveAvatar)
            .WithSummary("RemoveAvatar")
            .WithDescription("Removes the authenticated user's avatar.");

        return app;
    }

    private static Results<Ok<Result<UserDto>>, UnauthorizedHttpResult> GetSelf(IUserContextService userContextService)
    {
        if (userContextService.GetUser() is not { } user)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(new Result<UserDto>(new UserDto(user)));
    }

    private static async Task<Results<Ok<Result>, BadRequest<Result>, InternalServerError<Result>>> PatchSelf(
        [FromBody]
        JsonElement patch,
        ICrudService<User> crudService,
        IUserContextService userContextService
    )
    {
        var userId = userContextService.GetUserId();
        var result = await crudService.Patch(JsonFormatter.GetPatchDocument<User>(patch), userId);

        if (result.HasError && (
                result.HasErrorOfType<InvalidOperationException>()
                || result.HasErrorOfType<EntityValidationException>()
            ))
            return TypedResults.BadRequest(result);

        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Result>, BadRequest<Result>, UnauthorizedHttpResult>> UpdatePassword(
        [FromBody]
        UserPasswordUpdatePayload request,
        IUserService userService,
        IUserContextService userContextService
    )
    {
        var user = userContextService.GetUser();
        var result = await userService.UpdatePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (result.HasErrorOfType<PasswordMalformedException>())
            return TypedResults.BadRequest(result);
        if (result.HasErrorOfType<InvalidCredentialsException>())
            return TypedResults.Unauthorized();

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<Result<Document>>> UpdateAvatar(
        IFormFile avatar,
        IUserService userService,
        IUserContextService userContextService
    )
    {
        var user = userContextService.GetUser();
        var result = await userService.UpdateAvatar(user, avatar);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<Result>> RemoveAvatar(
        IUserService userService,
        IUserContextService userContextService
    )
    {
        var user = userContextService.GetUser();
        var result = await userService.RemoveUserAvatar(user);
        return TypedResults.Ok(result);
    }
}