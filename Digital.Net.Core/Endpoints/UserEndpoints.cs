using System.Text.Json;
using Digital.Net.Core.Endpoints.Dto;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Authentication.Exceptions;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.Users;
using Digital.Net.Core.Services.Users.Events;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Core.Endpoints;

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
        UserContextService userContextService
    )
    {
        if (userContextService.GetUser() is not { } user)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(new Result<bool>(user.IsAdmin));
    }

    private static Results<Ok<Result<UserDto>>, UnauthorizedHttpResult> GetSelf(UserContextService userContextService)
    {
        if (userContextService.GetUser() is not { } user)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(new Result<UserDto>(new UserDto(user)));
    }

    private static async Task<Results<Ok<Result>, BadRequest<Result>, InternalServerError<Result>>> PatchSelf(
        [FromBody]
        JsonElement patch,
        CrudService<DigitalContext, User> crudService,
        IAuditService auditService,
        UserContextService userContextService,
        CancellationToken ct
    )
    {
        var userId = userContextService.GetUserId();
        var result = await crudService.Patch(patch, userId, ct);
        var isBadRequest = result.HasErrorOfType<EntityValidationException>() ||
                           result.HasErrorOfType<InvalidOperationException>();

        if (result.HasError && !isBadRequest)
            return TypedResults.InternalServerError(result);

        await auditService.RegisterEventAsync(
            UserEvents.UpdateProfile,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId(),
            patch.GetRawText()
        );

        return result.HasError
            ? TypedResults.BadRequest(result)
            : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Result>, BadRequest<Result>, UnauthorizedHttpResult>> UpdatePassword(
        [FromBody]
        UserPasswordUpdatePayload request,
        UserService userService,
        UserContextService userContextService
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
        UserService userService,
        UserContextService userContextService
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
        UserService userService,
        UserContextService userContextService
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