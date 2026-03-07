using System.Linq.Expressions;
using Digital.Net.Authentication.Filters;
using Digital.Net.Controllers.Dto;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Crud.Controllers;
using Digital.Net.Entities.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Controllers.Controllers;

public static class AdministrationEndpoints
{
    public static IEndpointRouteBuilder MapAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("admin")
            .WithTags("Administration")
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
            .MapCrudPost<User, UserPayload>("user")
            .WithSummary("CreateUser")
            .WithDescription("Creates a new user with the provided information.");

        return app;
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