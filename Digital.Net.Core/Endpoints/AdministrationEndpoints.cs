using System.Linq.Expressions;
using System.Text.Json;
using Digital.Net.Core.Endpoints.Dto;
using Digital.Net.Core.Entities;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Authentication.Utils;
using Digital.Net.Core.Services.Crud.Extensions;
using Digital.Net.Core.Services.Pagination.Extensions;
using Digital.Net.Core.Services.Users;
using Digital.Net.Core.Services.Users.Events;
using Digital.Net.Core.Services.Users.Events.Types;
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

namespace Digital.Net.Core.Endpoints;

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
            .MapCrudGet<User, UserDto>("user")
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

        return app;
    }

    private static async Task<Results<Ok<Result<Guid>>, BadRequest<Result<Guid>>>> CreateUser(
        [FromBody]
        UserPayload payload,
        IUserService userService,
        IUserContextService userContextService,
        IAuditService auditService
    )
    {
        var result = await userService.CreateUserAsync(
            login: payload.Login,
            username: payload.Username,
            email: payload.Email,
            password: payload.Password
        );

        if (result.HasError)
            return TypedResults.BadRequest(result);

        await auditService.RegisterEventAsync(
            UserEvents.CreateUser,
            EventState.Success,
            result,
            userContextService.GetUserId(),
            JsonSerializer.Serialize(new AdminUserMutationEvent(payload, result.Value))
        );
        return TypedResults.Ok(result);
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
            IUserService userService,
            IUserContextService userContextService,
            IAuditService auditService
        )
    {
        var admin = userContextService.GetUser();

        if (!PasswordUtils.VerifyPassword(admin, payload.Password))
            return TypedResults.Unauthorized();

        var result = await userService.DeleteUserAsync(id);

        Func<EventState, Task> registerEvent = async state =>
            await auditService.RegisterEventAsync(
                UserEvents.DeleteUser,
                state,
                result,
                admin.Id,
                JsonSerializer.Serialize(new AdminUserMutationEvent { Id = id })
            );

        if (result.HasErrorOfType<CannotDeleteAdminException>())
        {
            await registerEvent(EventState.Failed);
            return TypedResults.StatusCode(403);
        }

        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        await registerEvent(EventState.Success);
        return TypedResults.Ok(result);
    }

    private static Expression<Func<User, bool>> PaginationFilter(
        this Expression<Func<User, bool>> predicate,
        UserQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Username))
            predicate = predicate.Add(x =>
                EF.Functions.Like(x.Username, $"{EfCoreQuery.EscapeLike(query.Username)}%")
            );
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
            IUserService userService,
            IUserContextService userContextService,
            IAuditService auditService
        )
    {
        var adminId = userContextService.GetUserId();
        var result = await userService.UpdateUserStatusAsync(id, payload.IsActive);

        Func<EventState, Task> registerEvent = async state =>
            await auditService.RegisterEventAsync(
                UserEvents.UpdateUserStatus,
                state,
                result,
                adminId,
                JsonSerializer.Serialize(new AdminUserMutationEvent { Id = id })
            );

        if (result.HasErrorOfType<CannotRevokeAdminException>())
        {
            await registerEvent(EventState.Failed);
            return TypedResults.StatusCode(403);
        }

        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        await registerEvent(EventState.Success);
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
            IUserService userService,
            IUserContextService userContextService,
            IAuditService auditService
        )
    {
        var admin = userContextService.GetUser();

        if (!PasswordUtils.VerifyPassword(admin, payload.Password))
            return TypedResults.Unauthorized();

        var result = await userService.UpdateUserRoleAsync(id, payload.IsAdmin);

        Func<EventState, Task> registerEvent = async state =>
            await auditService.RegisterEventAsync(
                UserEvents.UpdateUserRole,
                state,
                result,
                admin.Id,
                JsonSerializer.Serialize(new AdminUserMutationEvent { Id = id })
            );

        if (result.HasErrorOfType<CannotDemoteAdminException>())
        {
            await registerEvent(EventState.Failed);
            return TypedResults.StatusCode(403);
        }

        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        await registerEvent(EventState.Success);
        return TypedResults.Ok(result);
    }
}