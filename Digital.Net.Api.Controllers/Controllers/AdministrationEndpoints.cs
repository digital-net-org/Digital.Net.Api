using System.Linq.Expressions;
using Digital.Net.Api.Authentication.Filters;
using Digital.Net.Api.Controllers.Dto;
using Digital.Net.Api.Core.Predicates;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Crud.Controllers;
using Digital.Net.Api.Entities.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Controllers.Controllers;

public static class AdministrationEndpoints
{
    public static IEndpointRouteBuilder MapAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("admin")
            .WithTags("Administration")
            .RequireAuthentication(AuthorizeType.Any);

        controller.MapCrudGet<User, DigitalContext, UserDto>("user");
        controller.MapPaginationGet<User, DigitalContext, UserDto, UserQuery>("user", PaginationFilter);
        controller.MapCrudPatch<User, DigitalContext>("user");
        controller.MapCrudPost<User, DigitalContext, UserDto>("user");
        controller.MapCrudDelete<User, DigitalContext>("user");

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