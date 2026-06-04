using System.Linq.Expressions;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Models;
using Digital.Net.Core.Http.RateLimiters;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Core.Http.Services.Pagination.Extensions;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapCmsTagEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("cms/tags")
            .WithTags("CMS.Tags")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller.MapCrudSchema<CmsContext, Tag>();
        controller.MapCrudGet<CmsContext, Tag, TagDto>();
        controller.MapPaginationGet<CmsContext, Tag, TagDto, TagQuery>(filter: PaginationFilter);
        controller.MapCrudPost<CmsContext, Tag, TagPayload>();
        controller.MapCrudPatch<CmsContext, Tag>();
        controller.MapCrudDelete<CmsContext, Tag>();

        return app;
    }
    
    private static Expression<Func<Tag, bool>> PaginationFilter(
        Expression<Func<Tag, bool>> predicate,
        TagQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => EF.Functions.ILike(x.Name, $"{query.Name}%"));
        return predicate;
    }
}