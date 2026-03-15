using System.Linq.Expressions;
using System.Text.Json;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Crud.Extensions;
using Digital.Net.Core.Services.Pagination.Extensions;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Lib.Formatters;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Cms.Endpoints;

public static class CmsPageEndpoints
{
    public static IEndpointRouteBuilder MapCmsPageEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("cms/pages")
            .WithTags("CMS - Pages")
            .RequireRateLimiting(GlobalLimiter.Policy);

        controller
            .MapGet("path/{*path}", GetPageByPath)
            .WithSummary("GetByPath")
            .WithDescription("Retrieves a published page by its path. Requires Application authentication.")
            .RequireAuthentication(AuthorizeType.Application);

        controller
            .MapCrudSchema<CmsContext, Page>("")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapCrudGet<Page, PageDto>("")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapPaginationGet<CmsContext, Page, PageDto, PageQuery>("", PaginationFilter)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapPost("", CreatePage)
            .WithSummary("Create")
            .WithDescription("Creates a new page.")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapPatch("{id:guid}", UpdatePage)
            .WithSummary("Patch")
            .WithDescription("Updates a page by its ID.")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapDelete("{id:guid}", DeletePage)
            .WithSummary("Delete")
            .WithDescription("Deletes a page by its ID.")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        return app;
    }

    private static Results<Ok<PageDto>, NotFound> GetPageByPath(
        string path,
        CmsContext context
    )
    {
        var page = context.Pages.FirstOrDefault(p => p.Path == path && p.Published);
        return page is null ? TypedResults.NotFound() : TypedResults.Ok(new PageDto(page));
    }

    private static async Task<IResult> CreatePage(
        [FromBody]
        PagePayload payload,
        ICrudService<Page> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var entity = new Page { Path = payload.Path };
        var result = await crudService.Create(entity);
        await auditService.RegisterEventAsync(
            CmsEvents.CreatePage,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.BadRequest(result)
            : Results.Ok(result);
    }

    private static async Task<IResult> UpdatePage(
        Guid id,
        [FromBody]
        JsonElement patch,
        ICrudService<Page> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await crudService.Patch(patch.GetPatchDocument<Page>(), id);
        await auditService.RegisterEventAsync(
            CmsEvents.UpdatePage,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.BadRequest(result)
            : Results.Ok(result);
    }

    private static async Task<IResult> DeletePage(
        Guid id,
        ICrudService<Page> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await crudService.Delete(id);
        await auditService.RegisterEventAsync(
            CmsEvents.DeletePage,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.NotFound(result)
            : Results.Ok(result);
    }

    private static Expression<Func<Page, bool>> PaginationFilter(
        Expression<Func<Page, bool>> predicate,
        PageQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Path))
            predicate = predicate.Add(x => x.Path.StartsWith(query.Path));
        if (query.Published.HasValue)
            predicate = predicate.Add(x => x.Published == query.Published);
        if (query.Indexed.HasValue)
            predicate = predicate.Add(x => x.Indexed == query.Indexed);
        return predicate;
    }
}