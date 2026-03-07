using System.Linq.Expressions;
using Digital.Net.Authentication.Filters;
using Digital.Net.Controllers.Dto;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Crud.Controllers;
using Digital.Net.Entities.Models.Pages;
using Digital.Net.Entities.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Controllers.Controllers;

public static class PageEndpoints
{
    public static IEndpointRouteBuilder MapPageEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("page")
            .WithTags("Page");

        controller
            .MapGet("/path/{*path}", GetPageByPath)
            .WithSummary("GetByPath")
            .WithDescription("Retrieves a page meta and config by its path.");

        controller
            .MapCrudGet<Page, PageDto>("")
            .RequireAuthentication(AuthorizeType.Any);

        controller
            .MapPaginationGet<Page, PageDto, PageQuery>("", PaginationFilter)
            .RequireAuthentication(AuthorizeType.Any);

        controller
            .MapCrudPatch<Page>("")
            .RequireAuthentication(AuthorizeType.Any);

        controller
            .MapCrudPost<Page, PagePayload>("")
            .RequireAuthentication(AuthorizeType.Any);

        controller
            .MapCrudDelete<Page>("")
            .RequireAuthentication(AuthorizeType.Any);

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