using System.Linq.Expressions;
using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.RateLimiters;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Core.Http.Services.Pagination.Extensions;
using Digital.Net.Core.Services.Users;
using Digital.Net.Core.Services.Users.Exceptions;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Http.Endpoints;

public static class AdministrationEndpoints
{
    public static IEndpointRouteBuilder MapAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("admin")
            .WithTags("Administration")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey)
            .RequireAdmin();

        controller
            .MapCrudGet<DigitalContext, User, UserDto>("user")
            .WithSummary("GetUserById")
            .WithDescription("Retrieves a user by their unique identifier.");

        controller
            .MapPaginationGet<DigitalContext, User, UserDto, UserQuery>("user", PaginationFilter)
            .WithSummary("GetPaginatedUsers")
            .WithDescription("Retrieves a paginated list of users with filtering and sorting options.");

        controller
            .MapPost("user", CreateUser)
            .WithSummary("CreateUser")
            .WithDescription("Creates a new user with the provided information.");

        controller
            .MapDelete("user/{id:guid}", DeleteUser)
            .WithSummary("DeleteUser")
            .WithDescription("Deletes a user after admin password confirmation. Admin users cannot be deleted.");

        controller
            .MapPut("user/{id:guid}/status", UpdateUserStatus)
            .WithSummary("UpdateUserStatus")
            .WithDescription("Activates or deactivates a user. Admin users cannot be deactivated.");

        controller
            .MapPut("user/{id:guid}/role", UpdateUserRole)
            .WithSummary("UpdateUserRole")
            .WithDescription("Grants or revokes admin privileges. Existing admins cannot be demoted.");

        controller
            .MapPaginationGet<DigitalContext, AuthEvent, AuthEventDto, AuthEventQuery>("auth-events", PaginationFilter)
            .WithSummary("GetPaginatedAuthEvents")
            .WithDescription(
                "Retrieves the paginated audit log of authentication events " +
                "(login/logout/password changes), newest first by default."
            );

        return app;
    }

    private static async Task<Results<Ok<Result<Guid>>, BadRequest<Result<Guid>>>> CreateUser(
        [FromBody]
        UserPayload payload,
        UserService userService
    )
    {
        var result = await userService.CreateUserAsync(
            login: payload.Login,
            username: payload.Username,
            email: payload.Email,
            password: payload.Password
        );

        return result.HasError
            ? TypedResults.BadRequest(result)
            : TypedResults.Ok(result);
    }

    private static async
        Task<Results<
            Ok<Result>,
            BadRequest<Result>,
            NotFound<Result>,
            InternalServerError<Result>,
            StatusCodeHttpResult,
            UnauthorizedHttpResult>>
        DeleteUser(
            Guid id,
            [FromBody]
            UserDeletePayload payload,
            UserService userService,
            IUserAccessor userContextService
        )
    {
        var admin = userContextService.GetUser();

        if (!UserPassword.Verify(admin, payload.Password))
            return TypedResults.Unauthorized();

        var result = await userService.DeleteUserAsync(id);

        if (result.HasErrorOfType<CannotDeleteAdminException>())
            return TypedResults.StatusCode(403);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok(result);
    }

    private static Expression<Func<User, bool>> PaginationFilter(
        this Expression<Func<User, bool>> predicate,
        UserQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Username))
            predicate = predicate.Add(x => EF.Functions.Like(x.Username, $"{EFCoreUtils.EscapeLike(query.Username)}%"));
        if (!string.IsNullOrEmpty(query.Email))
            predicate = predicate.Add(x => x.Email.StartsWith(query.Email));
        if (query.IsActive.HasValue)
            predicate = predicate.Add(x => x.IsActive == query.IsActive);
        if (query.IsAdmin.HasValue)
            predicate = predicate.Add(x => x.IsAdmin == query.IsAdmin);
        return predicate;
    }

    private static async
        Task<Results<
            Ok<Result>,
            NotFound<Result>,
            InternalServerError<Result>,
            StatusCodeHttpResult>>
        UpdateUserStatus(
            Guid id,
            [FromBody]
            UserStatusPayload payload,
            UserService userService
        )
    {
        var result = await userService.UpdateUserStatusAsync(id, payload.IsActive);

        if (result.HasErrorOfType<CannotRevokeAdminException>())
            return TypedResults.StatusCode(403);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok(result);
    }

    private static async
        Task<Results<
            Ok<Result>,
            NotFound<Result>,
            InternalServerError<Result>,
            StatusCodeHttpResult,
            UnauthorizedHttpResult>>
        UpdateUserRole(
            Guid id,
            [FromBody]
            UserRolePayload payload,
            UserService userService,
            IUserAccessor userContextService
        )
    {
        var admin = userContextService.GetUser();

        if (!UserPassword.Verify(admin, payload.Password))
            return TypedResults.Unauthorized();

        var result = await userService.UpdateUserRoleAsync(id, payload.IsAdmin);
        if (result.HasErrorOfType<CannotDemoteAdminException>())
            return TypedResults.StatusCode(403);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok(result);
    }

    private static Expression<Func<AuthEvent, bool>> PaginationFilter(
        this Expression<Func<AuthEvent, bool>> predicate,
        AuthEventQuery query
    )
    {
        if (query.Type.HasValue)
            predicate = predicate.Add(x => x.Type == query.Type);
        if (query.Success.HasValue)
            predicate = predicate.Add(x => x.Success == query.Success);
        if (query.UserId.HasValue)
            predicate = predicate.Add(x => x.UserId == query.UserId);
        if (!string.IsNullOrEmpty(query.IpAddress))
            predicate = predicate.Add(x => x.IpAddress == query.IpAddress);
        return predicate;
    }
}
