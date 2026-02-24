using System.Text.Json;
using Digital.Net.Api.Authentication.Exceptions;
using Digital.Net.Api.Authentication.Filters;
using Digital.Net.Api.Authentication.Services.Authentication;
using Digital.Net.Api.Controllers.Dto;
using Digital.Net.Api.Core.Formatters;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Core.OpenApi;
using Digital.Net.Api.Entities.Crud;
using Digital.Net.Api.Entities.Crud.Controllers;
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

        group.MapCrudSchema<User>("");

        group
            .MapGet("/self", GetSelf)
            .WithDoc(d =>
            {
                d.Summary = "GetSelf";
                d.Description = "Retrieves the authenticated user's information.";
            });

        group
            .MapPatch("/self", PatchSelf)
            .WithDoc(d =>
            {
                d.Summary = "PatchSelf";
                d.Description =
                    "Updates the authenticated user's information using a JSON patch. Use the *User Schema* to get the available fields.";
            });

        group
            .MapPut("/self/password", UpdatePassword)
            .WithDoc(d =>
            {
                d.Summary = "UpdatePassword";
                d.Description = "Updates the authenticated user's password.";
            });

        group
            .MapPut("/self/avatar", UpdateAvatar)
            .WithDoc(d =>
            {
                d.Summary = "UpdateAvatar";
                d.Description = "Updates the authenticated user's avatar.";
            });

        group
            .MapDelete("/self/avatar", RemoveAvatar)
            .WithDoc(d =>
            {
                d.Summary = "RemoveAvatar";
                d.Description = "Removes the authenticated user's avatar.";
            });

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
            return Results.BadRequest(result);

        if (result.HasError)
            return Results.InternalServerError(result);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdatePassword(
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
        IUserService userService,
        IUserContextService userContextService
    )
    {
        var user = userContextService.GetUser();
        var result = await userService.RemoveUserAvatar(user);
        return Results.Ok(result);
    }
}