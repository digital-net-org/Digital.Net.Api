using System.Linq.Expressions;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Http.Services;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Core.Http.Security;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Core.Http.Services.Pagination.Extensions;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Http.Endpoints;

public static class ArticleEndpoints
{
    public static IEndpointRouteBuilder MapCmsArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("cms/articles")
            .WithTags("CMS.Articles")
            .RequireRateLimiting(RateLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapGet("slug/availability", GetSlugAvailability)
            .WithSummary("CheckSlugAvailability")
            .WithDescription(
                "Returns true if the slug is not yet used by any article. " +
                "ExcludeId scopes out a specific id (edition case)."
            );

        controller.MapCrudSchema<CmsContext, Article>();
        controller.MapCrudSchema<CmsContext, ArticleMedia>("media");
        controller.MapCrudGet<CmsContext, Article, ArticleDto>();
        controller.MapPaginationGet<CmsContext, Article, ArticleListDto, ArticleQuery>(filter: PaginationFilter);
        controller.MapCrudPost<CmsContext, Article, ArticlePayload>();
        controller.MapCrudPatch<CmsContext, Article>();
        controller.MapCrudDelete<CmsContext, Article>();

        return app;
    }

    private static async Task<Results<Ok<Result<bool>>, InternalServerError<Result<bool>>>>
        GetSlugAvailability(
            [FromQuery]
            string slug,
            [FromQuery]
            Guid? excludeId,
            ArticleService articleService,
            CancellationToken ct
        )
    {
        var result = await articleService.GetSlugAvailability(slug, excludeId, ct);
        return result.HasError
            ? TypedResults.InternalServerError(result)
            : TypedResults.Ok(result);
    }

    private static Expression<Func<Article, bool>> PaginationFilter(
        Expression<Func<Article, bool>> predicate,
        ArticleQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => EF.Functions.ILike(x.Title, $"{query.Name}%"));
        if (query.Published.HasValue)
            predicate = predicate.Add(x => x.PublishedAt != null == query.Published);
        if (query.TagId.HasValue)
            predicate = predicate.Add(x => x.Tags.Any(t => t.Id == query.TagId));
        if (query.PageId.HasValue)
            predicate = predicate.Add(x => x.PageId == query.PageId);
        return predicate;
    }
}