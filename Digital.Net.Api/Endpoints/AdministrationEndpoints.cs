using System.Linq.Expressions;
using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Api.RateLimiter.Limiters;
using Digital.Net.Api.Services.Auditing;
using Digital.Net.Api.Services.Authentication;
using Digital.Net.Api.Services.Authentication.Filters;
using Digital.Net.Api.Services.Users;
using Digital.Net.Api.Services.Users.Events;
using Digital.Net.Core.Messages;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Crud.Enpoints;
using Digital.Net.Entities.Models.Events;
using Digital.Net.Entities.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Endpoints;

public static class AdministrationEndpoints
{
    public static IEndpointRouteBuilder MapAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("admin")
            .WithTags("Administration")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Any)
            .RequireAdmin();

        controller
            .MapCrudGet<User, UserDto>("user")
            .WithSummary("GetUserById")
            .WithDescription("Retrieves a user by their unique identifier.");

        controller
            .MapPaginationGet<User, UserDto, UserQuery>("user", PaginationFilter)
            .WithSummary("GetPaginatedUsers")
            .WithDescription("Retrieves a paginated list of users with filtering and sorting options.");

        controller
            .MapPost("user", CreateUser)
            .WithSummary("CreateUser")
            .WithDescription("Creates a new user with the provided information.");

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
            userContextService.GetUserId()
        );
        return TypedResults.Ok(result);
    }

    private static Expression<Func<User, bool>>
        PaginationFilter(
            this Expression<Func<User, bool>> predicate,
            UserQuery query
        )
    {
        if (!string.IsNullOrEmpty(query.Username))
            predicate = predicate.Add(x => x.Username.StartsWith(query.Username));
        if (!string.IsNullOrEmpty(query.Email))
            predicate = predicate.Add(x => x.Email.StartsWith(query.Email));
        if (query.IsActive.HasValue)
            predicate = predicate.Add(x => x.IsActive == query.IsActive);
        return predicate;
    }
}