using System.Text.Json;
using Digital.Net.Api.Authentication.Exceptions;
using Digital.Net.Api.Authentication.Filters;
using Digital.Net.Api.Authentication.Services.Authentication;
using Digital.Net.Api.Controllers.Dto;
using Digital.Net.Api.Core.Formatters;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Crud;
using Digital.Net.Api.Entities.Exceptions;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Services.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Controllers.Controllers;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/user")
            .WithTags("User")
            .RequireAuthentication(AuthorizeType.Any);

        group.MapGet("/self", GetSelf);
        group.MapPatch("/self", PatchSelf);
        group.MapPut("/self/password", UpdatePassword);
        group.MapPut("/self/avatar", UpdateAvatar);
        group.MapDelete("/self/avatar", RemoveAvatar);

        return app;
    }

    private static IResult GetSelf(IUserContextService userContextService)
    {
        if (userContextService.GetUser() is not { } user)
            return Results.Unauthorized();

        return Results.Ok(new Result<UserDto>(new UserDto(user)));
    }

    private static async Task<IResult> PatchSelf(
        [FromBody]
        JsonElement patch,
        ICrudService<User, DigitalContext> crudService,
        IUserContextService userContextService
    )
    {
        var userId = userContextService.GetUserId();
        var result = await crudService.Patch(JsonFormatter.GetPatchDocument<User>(patch), userId);

        if (result.HasError && (
                result.HasErrorOfType<InvalidOperationException>()
                || result.HasErrorOfType<EntityValidationException>()
            ))
            return Results.BadRequest(result);

        if (result.HasError)
            return Results.InternalServerError(result);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdatePassword(
        Guid id,
        [FromBody]
        UserPasswordUpdatePayload request,
        IUserService userService,
        IUserContextService userContextService
    )
    {
        var user = userContextService.GetUser();
        var result = await userService.UpdatePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (result.HasErrorOfType<PasswordMalformedException>())
            return Results.BadRequest(result);
        if (result.HasErrorOfType<InvalidCredentialsException>())
            return Results.Unauthorized();

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateAvatar(
        Guid id,
        IFormFile avatar,
        IUserService userService,
        IUserContextService userContextService
    )
    {
        var user = userContextService.GetUser();
        var result = await userService.UpdateAvatar(user, avatar);
        return Results.Ok(result);
    }

    private static async Task<IResult> RemoveAvatar(
        Guid id,
        IUserService userService,
        IUserContextService userContextService
    )
    {
        var user = userContextService.GetUser();
        var result = await userService.RemoveUserAvatar(user);
        return Results.Ok(result);
    }
}