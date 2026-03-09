using System.Text.Json;
using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Api.RateLimiter.Limiters;
using Digital.Net.Api.Services.Authentication;
using Digital.Net.Api.Services.Authentication.Exceptions;
using Digital.Net.Api.Services.Authentication.Filters;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.Documents.Exceptions;
using Digital.Net.Api.Services.Users;
using Digital.Net.Core.Formatters;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Crud;
using Digital.Net.Entities.Crud.Enpoints;
using Digital.Net.Entities.Exceptions;
using Digital.Net.Entities.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/user")
            .WithTags("User")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Any);

        group.MapCrudSchema<User>("");

        group
            .MapGet("/self", GetSelf)
            .WithSummary("GetSelf")
            .WithDescription("Retrieves the authenticated user's information.");

        group
            .MapPatch("/self", PatchSelf)
            .WithSummary("PatchSelf")
            .WithDescription(
                "Updates the authenticated user's information using a JSON patch. Use the *User Schema* to get the available fields.");

        group
            .MapPut("/self/password", UpdatePassword)
            .WithSummary("UpdatePassword")
            .WithDescription("Updates the authenticated user's password.");

        group
            .MapPut("/self/avatar", UpdateAvatar)
            .DisableAntiforgery() // Not needed as we don't use session cookie
            .WithSummary("UpdateAvatar")
            .WithDescription("Updates the authenticated user's avatar.");

        group
            .MapDelete("/self/avatar", RemoveAvatar)
            .WithSummary("RemoveAvatar")
            .WithDescription("Removes the authenticated user's avatar.");

        group
            .MapGet("/{id:guid}/avatar", GetUserAvatar)
            .WithSummary("GetUserAvatar")
            .WithDescription("Retrieves the avatar image file for a given user.");

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
        var result = await crudService.Patch(patch.GetPatchDocument<User>(), userId);

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

    private static async Task<Results<Ok, BadRequest<Result>, InternalServerError<Result>>> UpdateAvatar(
        IFormFile avatar,
        IUserService userService,
        IUserContextService userContextService
    )
    {
        var user = userContextService.GetUser();
        var result = await userService.UpdateAvatar(user, avatar);

        if (result.HasErrorOfType<TooHeavyException>() || result.HasErrorOfType<UnsupportedFormatException>())
            return TypedResults.BadRequest(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, InternalServerError<Result>>> RemoveAvatar(
        IUserService userService,
        IUserContextService userContextService
    )
    {
        var user = userContextService.GetUser();
        var result = await userService.RemoveUserAvatar(user);

        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok();
    }

    private static async Task<Results<FileContentHttpResult, NotFound, InternalServerError, StatusCodeHttpResult>>
        GetUserAvatar(
            Guid id,
            IUserService userService,
            IDocumentCacheService documentCacheService
        )
    {
        var result = await userService.GetUserAvatarDocumentAsync(id);

        if (result.HasError || result.Value is null)
            return TypedResults.NotFound();

        var cacheResult = documentCacheService.GetCachedDocumentFile(result.Value);

        if (cacheResult.HasErrorOfType<DocumentNotFoundException>())
            return TypedResults.NotFound();
        if (cacheResult.HasError)
            return TypedResults.InternalServerError();
        if (cacheResult.Value is not FileContentResult fileContentResult)
            return TypedResults.StatusCode(304);

        return TypedResults.File(
            fileContentResult.FileContents,
            fileContentResult.ContentType
        );
    }
}