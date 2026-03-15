using System.Linq.Expressions;
using System.Text.Json;
using Digital.Net.Api.RateLimiter.Limiters;
using Digital.Net.Api.Services.Auditing;
using Digital.Net.Api.Services.Authentication;
using Digital.Net.Api.Services.Authentication.Filters;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models;
using Digital.Net.Core.Formatters;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Crud;
using Digital.Net.Entities.Crud.Endpoints;
using Digital.Net.Entities.Models.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Cms.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapCmsTagEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("cms/tags")
            .WithTags("CMS - Tags")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapCrudSchema<CmsContext, Tag>("");

        controller
            .MapCrudGet<Tag, TagDto>("");

        controller
            .MapPaginationGet<CmsContext, Tag, TagDto, TagQuery>("", PaginationFilter);

        controller
            .MapPost("", CreateTag)
            .WithSummary("Create")
            .WithDescription("Creates a new tag.");

        controller
            .MapPatch("{id:guid}", UpdateTag)
            .WithSummary("Patch")
            .WithDescription("Updates a tag by its ID.");

        controller
            .MapDelete("{id:guid}", DeleteTag)
            .WithSummary("Delete")
            .WithDescription("Deletes a tag by its ID.");

        return app;
    }

    private static async Task<IResult> CreateTag(
        [FromBody]
        TagPayload payload,
        ICrudService<Tag> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var entity = new Tag { Name = payload.Name, Color = payload.Color };
        var result = await crudService.Create(entity);
        await auditService.RegisterEventAsync(
            CmsEvents.CreateTag,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.BadRequest(result)
            : Results.Ok(result);
    }

    private static async Task<IResult> UpdateTag(
        Guid id,
        [FromBody]
        JsonElement patch,
        ICrudService<Tag> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await crudService.Patch(patch.GetPatchDocument<Tag>(), id);
        await auditService.RegisterEventAsync(
            CmsEvents.UpdateTag,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.BadRequest(result)
            : Results.Ok(result);
    }

    private static async Task<IResult> DeleteTag(
        Guid id,
        ICrudService<Tag> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await crudService.Delete(id);
        await auditService.RegisterEventAsync(
            CmsEvents.DeleteTag,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.NotFound(result)
            : Results.Ok(result);
    }

    private static Expression<Func<Tag, bool>> PaginationFilter(
        Expression<Func<Tag, bool>> predicate,
        TagQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => x.Name.StartsWith(query.Name));
        return predicate;
    }
}