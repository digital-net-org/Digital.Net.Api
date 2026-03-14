using System.Linq.Expressions;
using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Api.RateLimiter.Limiters;
using Digital.Net.Api.Services.Authentication.Filters;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Crud.Enpoints;
using Digital.Net.Entities.Models.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Endpoints;

public static class EventsEndpoints
{
    public static IEndpointRouteBuilder MapEventsEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("events")
            .WithTags("Events")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey)
            .RequireAdmin();

        controller
            .MapCrudGet<Event, EventDto>("")
            .WithSummary("GetEventById")
            .WithDescription("Retrieves an audit event by its unique identifier.");

        controller
            .MapPaginationGet<Event, EventDto, EventQuery>("", PaginationFilter)
            .WithSummary("GetPaginatedEvents")
            .WithDescription("Retrieves a paginated, filterable list of audit events.");

        return app;
    }

    private static Expression<Func<Event, bool>> PaginationFilter(
        Expression<Func<Event, bool>> predicate,
        EventQuery query
    )
    {
        if (query.UserId.HasValue)
            predicate = predicate.Add(e => e.UserId == query.UserId);
        if (!string.IsNullOrEmpty(query.EventType))
            predicate = predicate.Add(e => e.Name == query.EventType);
        return predicate;
    }
}
