using System.Linq.Expressions;
using System.Text.Json;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Cms.Services.Pages;
using Digital.Net.Cms.Services.Pages.Dto;
using Digital.Net.Cms.Services.Pages.OpenGraph;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Pagination.Extensions;
using Digital.Net.Lib.Exceptions.types;
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
            .WithTags("CMS.Pages")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapGet("path/availability", GetPathAvailability)
            .WithSummary("CheckPathAvailability")
            .WithDescription(
                "Returns true if the path is not yet used by any page. ExcludeId scopes out a specific id (edition case)."
            );

        controller.MapCrudSchema<CmsContext, Page>();
        controller.MapCrudSchema<CmsContext, Sheet>("sheet");
        controller.MapCrudSchema<CmsContext, OpenGraphEntry>("open-graph-entry");
        controller
            .MapGet("open-graph-values/schema", GetOpenGraphSchema)
            .WithSummary("GetOpenGraphValuesSchema")
            .WithDescription("Returns the list of valid OpenGraph property keys.");

        controller.MapCrudGet<CmsContext, Page, PageDto>();
        controller.MapPaginationGet<CmsContext, Page, PageDto, PageQuery>(filter: PaginationFilter);
        controller.MapCrudDelete<CmsContext, Page>(eventType: CmsEvents.DeletePage);
        controller
            .MapPatch("{id:guid}", UpdatePage)
            .WithSummary("Patch")
            .WithDescription(
                "Applies a JSON Patch to an entity identified by its ID. Returns the patched entity as the specified DTO type. Use the *Schema* endpoint to get the available fields."
            );
        controller
            .MapPost("", CreatePage)
            .WithSummary("Create")
            .WithDescription(
                "Creates a new entity with the provided payload. Returns the created entity as the specified DTO type."
            );
        
        controller
            .MapGet("{id:guid}/sheets", GetPageSheets)
            .WithSummary("GetPageSheets")
            .WithDescription("Retrieves every sheet owned by the page, ordered by load order.");

        controller
            .MapGet("{id:guid}/open-graph", GetPageOpenGraph)
            .WithSummary("GetPageOpenGraph")
            .WithDescription("Retrieves every OpenGraph entry owned by the page, ordered by index.");
        
        var publicController = app
            .MapGroup("cms/pages/public")
            .WithTags("CMS.Pages")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        publicController
            .MapGet("path", GetPublicPageByPath)
            .WithSummary("GetByPath")
            .WithDescription(
                "Retrieves a published page by its path (passed as a query string). Requires Application authentication."
            );

        publicController
            .MapGet("{id:guid}/sheets", GetPublicPageSheets)
            .WithSummary("GetPageSheets")
            .WithDescription("Retrieves every published sheet owned by the page, ordered by load order.");

        publicController
            .MapGet("{id:guid}/sheets/{sheetId:guid}", GetPublicPageSheetResource)
            .WithSummary("GetPageSheetResource")
            .WithDescription(
                "Serves the raw content of a published sheet scoped to its page, with the matching Content-Type."
            );

        return app;
    }

    private static async Task<Results<Ok<Result<PagePublicDto>>, NotFound<Result<PagePublicDto>>>> GetPublicPageByPath(
        [FromQuery]
        string path,
        PagePublicService pagePublicService
    )
    {
        var result = await pagePublicService.GetPageByPath(path);
        return result.HasErrorOfType<ResourceNotFoundException>()
            ? TypedResults.NotFound(result)
            : TypedResults.Ok(result);
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
    
    private static async
        Task<Results<Ok<Result<List<PageSheetDto>>>, NotFound<Result<List<PageSheetDto>>>>>
        GetPageSheets(
            Guid id,
            CrudService<CmsContext, Page> crudService,
            CancellationToken ct
        )
    {
        var result = await crudService.GetChildren<Sheet, PageSheet, PageSheetDto>(id, ct);
        return result.HasErrorOfType<ResourceNotFoundException>()
            ? TypedResults.NotFound(result)
            : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Result<List<OpenGraphEntryDto>>>, NotFound<Result<List<OpenGraphEntryDto>>>>>
        GetPageOpenGraph(
            Guid id,
            CrudService<CmsContext, Page> crudService,
            CancellationToken ct
        )
    {
        var result = await crudService.GetChildren<OpenGraphEntry, PageOpenGraph, OpenGraphEntryDto>(id, ct);
        return result.HasErrorOfType<ResourceNotFoundException>()
            ? TypedResults.NotFound(result)
            : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Result<Guid>>, BadRequest<Result<Guid>>, InternalServerError<Result<Guid>>>>
        CreatePage(
            [FromBody]
            PagePayload payload,
            PageCrudService pageCrudService,
            UserContextService userContextService
        )
    {
        var result = await pageCrudService.CreatePage(payload, userContextService.GetUserId());
        if (result.HasErrorOfType<EntityValidationException>())
            return TypedResults.BadRequest(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);
        
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Result>, BadRequest<Result>, NotFound<Result>, InternalServerError<Result>>>
        UpdatePage(
            Guid id,
            [FromBody]
            JsonElement patch,
            PageCrudService pageCrudService,
            UserContextService userContextService
        )
    {
        var result = await pageCrudService.PatchPage(patch, id, userContextService.GetUserId());
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound(result);
        if (result.HasErrorOfType<EntityValidationException>())
            return TypedResults.BadRequest(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok(result);
    }

    private static Ok<Result<IReadOnlyList<OpenGraphPropertySchema>>> GetOpenGraphSchema() =>
        TypedResults.Ok(new Result<IReadOnlyList<OpenGraphPropertySchema>>(OpenGraphProperties.Schema));

    private static async
        Task<Results<Ok<Result<List<PageSheetInfoDto>>>, InternalServerError<Result<List<PageSheetInfoDto>>>, NotFound>>
        GetPublicPageSheets(
        Guid id,
        PagePublicService pagePublicService
    )
    {
        var result = await pagePublicService.GetPageSheetInfos(id);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound();
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<ContentHttpResult, InternalServerError, NotFound>> GetPublicPageSheetResource(
        Guid id,
        Guid sheetId,
        PagePublicService pagePublicService
    )
    {
        var result = await pagePublicService.GetPageSheetResource(id, sheetId);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound();
        if (result.HasError)
            return TypedResults.InternalServerError();

        return TypedResults.Content(result.Value.content, result.Value.contentType);
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