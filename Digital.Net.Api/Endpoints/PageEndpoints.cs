using System.Linq.Expressions;
using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Api.RateLimiter.Limiters;
using Digital.Net.Api.Services.Authentication.Filters;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Crud.Enpoints;
using Digital.Net.Entities.Models.Pages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Endpoints;

public static class PageEndpoints
{
    public static IEndpointRouteBuilder MapPageEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("page")
            .WithTags("Page")
            .RequireRateLimiting(GlobalLimiter.Policy);

        controller
            .MapGet("/path/{*path}", GetPageByPath)
            .WithSummary("GetByPath")
            .WithDescription("Retrieves a page meta and config by its path.");

        controller
            .MapCrudGet<Page, PageDto>("")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapPaginationGet<Page, PageDto, PageQuery>("", PaginationFilter)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapCrudPatch<Page>("")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapCrudPost<Page, PagePayload>("")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapCrudDelete<Page>("")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        return app;
    }

    private static Results<Ok<PageDto>, NotFound> GetPageByPath(
        string path,
        DigitalContext context
    )
    {
        var page = context.Pages
            .Where(p => p.Path == path && p.IsPublished)
            .FirstOrDefault();
        if (page is null)
            return TypedResults.NotFound();

        var result = new PageDto(page);
        return TypedResults.Ok(result);
    }

    private static Expression<Func<Page, bool>> PaginationFilter(
        this Expression<Func<Page, bool>> predicate,
        PageQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Title))
            predicate = predicate.Add(x => x.Title.StartsWith(query.Title));
        if (query.IsPublished.HasValue)
            predicate = predicate.Add(x => x.IsPublished == query.IsPublished);
        return predicate;
    }
}