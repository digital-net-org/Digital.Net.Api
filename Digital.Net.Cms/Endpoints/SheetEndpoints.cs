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
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Endpoints;

public static class SheetEndpoints
{
    private static readonly string[] ValidTypes = ["css", "js"];

    public static IEndpointRouteBuilder MapCmsSheetEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("cms/sheets")
            .WithTags("CMS - Sheets")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapCrudSchema<CmsContext, Sheet>("");

        controller
            .MapCrudGet<Sheet, SheetDto>("");

        controller
            .MapPaginationGet<CmsContext, Sheet, SheetDto, SheetQuery>("", PaginationFilter);

        controller
            .MapPost("", CreateSheet)
            .WithSummary("Create")
            .WithDescription("Creates a new sheet.");

        controller
            .MapPatch("{id:guid}", UpdateSheet)
            .WithSummary("Patch")
            .WithDescription("Updates a sheet by its ID.");

        controller
            .MapDelete("{id:guid}", DeleteSheet)
            .WithSummary("Delete")
            .WithDescription("Deletes a sheet by its ID.");

        var associations = app
            .MapGroup("cms/pages/{pageId:guid}/sheets")
            .WithTags("CMS - Sheets")
            .RequireRateLimiting(GlobalLimiter.Policy);

        associations
            .MapGet("", GetPageSheets)
            .WithSummary("GetPageSheets")
            .WithDescription(
                "Retrieves the list of published sheets associated to a page, ordered by load order. "
                + "Requires Application authentication."
            )
            .RequireAuthentication(AuthorizeType.Application);

        associations
            .MapPost("", AssociateSheet)
            .WithSummary("AssociateSheet")
            .WithDescription("Associates a sheet to a page with a load order.")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        associations
            .MapDelete("{sheetId:guid}", DissociateSheet)
            .WithSummary("DissociateSheet")
            .WithDescription("Removes a sheet association from a page.")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        app
            .MapGroup("cms")
            .WithTags("CMS - Sheets")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .MapGet("resource/{id:guid}", GetResource)
            .WithSummary("GetResource")
            .WithDescription("Serves the content of a published sheet with the appropriate Content-Type.")
            .RequireAuthentication(AuthorizeType.Application);

        return app;
    }

    private static async Task<IResult> CreateSheet(
        [FromBody]
        SheetPayload payload,
        ICrudService<Sheet> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        if (!ValidTypes.Contains(payload.Type))
            return Results.BadRequest($"Invalid type '{payload.Type}'. Must be 'css' or 'js'.");

        var entity = new Sheet
        {
            Name = payload.Name,
            Type = payload.Type,
            Content = payload.Content
        };
        var result = await crudService.Create(entity);
        await auditService.RegisterEventAsync(
            CmsEvents.CreateSheet,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.BadRequest(result)
            : Results.Ok(result);
    }

    private static async Task<IResult> UpdateSheet(
        Guid id,
        [FromBody]
        JsonElement patch,
        ICrudService<Sheet> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await crudService.Patch(patch.GetPatchDocument<Sheet>(), id);
        await auditService.RegisterEventAsync(
            CmsEvents.UpdateSheet,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.BadRequest(result)
            : Results.Ok(result);
    }

    private static async Task<IResult> DeleteSheet(
        Guid id,
        ICrudService<Sheet> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await crudService.Delete(id);
        await auditService.RegisterEventAsync(
            CmsEvents.DeleteSheet,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.NotFound(result)
            : Results.Ok(result);
    }

    private static async Task<Results<Ok<Result<List<PageSheetDto>>>, NotFound>> GetPageSheets(
        Guid pageId,
        CmsContext context
    )
    {
        var page = await context.Pages.FindAsync(pageId);
        if (page is null)
            return TypedResults.NotFound();

        var sheets = await context.PageSheets
            .Include(ps => ps.Sheet)
            .Where(ps => ps.PageId == pageId && ps.Sheet.Published)
            .OrderBy(ps => ps.LoadOrder)
            .Select(ps => new PageSheetDto(ps))
            .ToListAsync();

        return TypedResults.Ok(new Result<List<PageSheetDto>>(sheets));
    }

    private static async Task<IResult> AssociateSheet(
        Guid pageId,
        [FromBody]
        PageSheetPayload payload,
        CmsContext context
    )
    {
        var page = await context.Pages.FindAsync(pageId);
        if (page is null)
            return Results.NotFound("Page not found.");

        var sheet = await context.Sheets.FindAsync(payload.SheetId);
        if (sheet is null)
            return Results.NotFound("Sheet not found.");

        var existing = await context.PageSheets
            .FirstOrDefaultAsync(ps => ps.PageId == pageId && ps.SheetId == payload.SheetId);
        if (existing is not null)
            return Results.Conflict("Sheet is already associated to this page.");

        var pageSheet = new PageSheet
        {
            PageId = pageId,
            SheetId = payload.SheetId,
            LoadOrder = payload.LoadOrder
        };
        context.PageSheets.Add(pageSheet);
        await context.SaveChangesAsync();

        return Results.Ok();
    }

    private static async Task<IResult> DissociateSheet(
        Guid pageId,
        Guid sheetId,
        CmsContext context
    )
    {
        var pageSheet = await context.PageSheets
            .FirstOrDefaultAsync(ps => ps.PageId == pageId && ps.SheetId == sheetId);
        if (pageSheet is null)
            return Results.NotFound();

        context.PageSheets.Remove(pageSheet);
        await context.SaveChangesAsync();

        return Results.Ok();
    }

    private static async Task<Results<ContentHttpResult, NotFound>> GetResource(
        Guid id,
        CmsContext context
    )
    {
        var sheet = await context.Sheets.FirstOrDefaultAsync(s => s.Id == id && s.Published);
        if (sheet is null)
            return TypedResults.NotFound();

        var contentType = sheet.Type switch
        {
            "css" => "text/css",
            "js" => "application/javascript",
            _ => "text/plain"
        };

        return TypedResults.Content(sheet.Content, contentType);
    }

    private static Expression<Func<Sheet, bool>> PaginationFilter(
        Expression<Func<Sheet, bool>> predicate,
        SheetQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => x.Name.StartsWith(query.Name));
        if (!string.IsNullOrEmpty(query.Type))
            predicate = predicate.Add(x => x.Type == query.Type);
        if (query.Published.HasValue)
            predicate = predicate.Add(x => x.Published == query.Published);
        return predicate;
    }
}
