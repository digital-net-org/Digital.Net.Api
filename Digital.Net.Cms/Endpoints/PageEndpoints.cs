using System.Linq.Expressions;
using System.Text.Json;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models;
using Digital.Net.Cms.Services.Pages;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Crud.Extensions;
using Digital.Net.Core.Services.Pagination.Extensions;
using Digital.Net.Lib.Formatters;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Endpoints;

public static class PageEndpoints
{
    public static IEndpointRouteBuilder MapCmsPageEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("cms/pages")
            .WithTags("CMS - Pages")
            .RequireRateLimiting(GlobalLimiter.Policy);

        controller
            .MapGet("path", GetPageByPath)
            .WithSummary("GetByPath")
            .WithDescription(
                "Retrieves a published page by its path (passed as a query string). Requires Application authentication.")
            .RequireAuthentication(AuthorizeType.Application);

        controller
            .MapGet("path/availability", GetPathAvailability)
            .WithSummary("CheckPathAvailability")
            .WithDescription(
                "Returns true if the path is not yet used by any page. ExcludeId scopes out a specific id (edition case).")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapCrudSchema<CmsContext, Page>("")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapGet("schema/open-graph", GetOpenGraphSchema)
            .WithSummary("GetOpenGraphSchema")
            .WithDescription("Returns the list of valid OpenGraph properties with an allowMultiple flag.")
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

    private static async Task<Results<Ok<Result<PageDto>>, NotFound>> GetPageByPath(
        [FromQuery]
        string path,
        CmsContext context
    )
    {
        var page = await context.Pages.FirstOrDefaultAsync(p => p.Path == path && p.Published);
        return page is null ? TypedResults.NotFound() : TypedResults.Ok(new Result<PageDto>(new PageDto(page)));
    }

    private static async Task<Ok<Result<bool>>> GetPathAvailability(
        [FromQuery]
        string path,
        [FromQuery]
        Guid? excludeId,
        CmsContext context
    )
    {
        var taken = await context.Pages.AnyAsync(p => p.Path == path && (excludeId == null || p.Id != excludeId));
        return TypedResults.Ok(new Result<bool>(!taken));
    }

    private static async Task<IResult> CreatePage(
        [FromBody]
        PagePayload payload,
        ICrudService<Page> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var consistency = PagePayloadValidator.ValidateEntityTypeConsistency(payload.Path, payload.EntityType);
        if (consistency.HasError)
        {
            await auditService.RegisterEventAsync(
                CmsEvents.CreatePage, EventState.Failed, consistency, userContextService.GetUserId()
            );
            return Results.BadRequest(consistency);
        }

        var entity = new Page { Path = payload.Path, EntityType = payload.EntityType };
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
        IUserContextService userContextService,
        CmsContext context
    )
    {
        var current = await context.Pages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (current is null) return Results.NotFound();

        var consistency = PagePayloadValidator.ValidatePatch(patch, current);
        if (consistency.HasError)
        {
            await auditService.RegisterEventAsync(
                CmsEvents.UpdatePage, 
                EventState.Failed, 
                consistency, 
                userContextService.GetUserId()
            );
            return Results.BadRequest(consistency);
        }

        var result = await crudService.Patch(
            PagePayloadValidator.NormalizePatch(patch).GetPatchDocument<Page>(), 
            id
        );
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

    private static Ok<Result<IReadOnlyList<OpenGraphPropertySchema>>> GetOpenGraphSchema() =>
        TypedResults.Ok(new Result<IReadOnlyList<OpenGraphPropertySchema>>(OpenGraphProperties.Schema));

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
