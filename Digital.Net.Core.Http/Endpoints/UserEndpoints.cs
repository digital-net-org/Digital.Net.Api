using System.Text.Json;
using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Accessors;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.RateLimiters;
using Digital.Net.Core.Http.Services.Authentication;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Core.Http.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.Users;
using Digital.Net.Core.Services.Users.Exceptions;
using Digital.Net.Lib.Files;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Core.Http.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/user")
            .WithTags("User")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        group.MapCrudSchema<DigitalContext, User>("");

        group
            .MapGet("/self", GetSelf)
            .WithSummary("GetSelf")
            .WithDescription("Retrieves the authenticated user's information.");

        group
            .MapPatch("/self", PatchSelf)
            .WithSummary("PatchSelf")
            .WithDescription(
                "Updates the authenticated user's information using a JSON patch. Use the *User Schema* to get the available fields."
            );

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
            .MapGet("/self/is-admin", GetSelfIsAdmin)
            .WithSummary("GetSelfIsAdmin")
            .WithDescription("Returns whether the authenticated user has admin privileges.");

        group
            .MapGet("/{id:guid}/avatar", GetUserAvatar)
            .WithSummary("GetUserAvatar")
            .WithDescription("Retrieves the avatar image file for a given user.");

        return app;
    }

    private static Results<Ok<Result<bool>>, UnauthorizedHttpResult> GetSelfIsAdmin(
        IUserAccessor userContextService
    )
    {
        if (userContextService.GetUser() is not { } user)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(new Result<bool>(user.IsAdmin));
    }

    private static Results<Ok<Result<UserDto>>, UnauthorizedHttpResult> GetSelf(IUserAccessor userContextService)
    {
        if (userContextService.GetUser() is not { } user)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(new Result<UserDto>(new UserDto(user)));
    }

    private static async Task<Results<Ok<Result>, BadRequest<Result>, InternalServerError<Result>>> PatchSelf(
        [FromBody]
        JsonElement patch,
        CrudService<DigitalContext, User> crudService,
        IUserAccessor userContextService,
        CancellationToken ct
    )
    {
        var userId = userContextService.GetUserId();
        var result = await crudService.Patch(patch, userId, ct);
        var isBadRequest = result.HasErrorOfType<EntityValidationException>() ||
                           result.HasErrorOfType<InvalidOperationException>();

        if (result.HasError && !isBadRequest)
            return TypedResults.InternalServerError(result);

        return result.HasError
            ? TypedResults.BadRequest(result)
            : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Result>, BadRequest<Result>, UnauthorizedHttpResult>> UpdatePassword(
        [FromBody]
        UserPasswordUpdatePayload request,
        UserService userService,
        AuthEventService authEventService,
        IUserAccessor userContextService,
        HttpContext ctx
    )
    {
        var user = userContextService.GetUser();
        var result = await userService.UpdatePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        await authEventService.RecordAsync(
            AuthEventType.PasswordChange,
            !result.HasError,
            ctx.GetRemoteIpAddress(),
            ctx.GetUserAgent(),
            user.Id
        );

        if (result.HasErrorOfType<PasswordMalformedException>())
            return TypedResults.BadRequest(result);
        if (result.HasErrorOfType<InvalidCredentialsException>())
            return TypedResults.Unauthorized();

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok, BadRequest<Result>, InternalServerError<Result>>> UpdateAvatar(
        IFormFile avatar,
        UserService userService,
        IUserAccessor userContextService
    )
    {
        var user = userContextService.GetUser();
        await using var stream = avatar.OpenReadStream();
        var definition = new FileDefinition
        {
            FileName = avatar.FileName,
            MimeType = avatar.ContentType,
            FileSize = avatar.Length
        };
        var result = await userService.UpdateAvatar(user, stream, definition);

        if (result.HasErrorOfType<TooHeavyException>() || result.HasErrorOfType<UnsupportedFormatException>())
            return TypedResults.BadRequest(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, InternalServerError<Result>>> RemoveAvatar(
        UserService userService,
        IUserAccessor userContextService
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
            UserService userService,
            DocumentCacheService documentCacheService
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