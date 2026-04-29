using System.Linq.Expressions;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Pagination.Extensions;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

        controller.MapCrudSchema<CmsContext, Tag>();
        controller.MapCrudGet<CmsContext, Tag, TagDto>();
        controller.MapPaginationGet<CmsContext, Tag, TagDto, TagQuery>(filter: PaginationFilter);
        controller.MapCrudPost<CmsContext, Tag, TagPayload>(eventType: CmsEvents.CreateTag);
        controller.MapCrudPatch<CmsContext, Tag>(eventType: CmsEvents.UpdateTag);
        controller.MapCrudDelete<CmsContext, Tag>(eventType: CmsEvents.DeleteTag);

        return app;
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