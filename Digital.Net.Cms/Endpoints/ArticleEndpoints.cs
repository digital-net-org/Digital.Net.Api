using System.Linq.Expressions;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Cms.Services.Articles;
using Digital.Net.Cms.Services.Articles.Dto;
using Digital.Net.Core.RateLimiter.Limiters;
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

public static class ArticleEndpoints
{
    public static IEndpointRouteBuilder MapCmsArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("cms/articles")
            .WithTags("CMS.Articles")
            .RequireRateLimiting(GlobalLimiter.Policy)
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
        controller.MapPaginationGet<CmsContext, Article, ArticleListDto, ArticleQuery>(
            filter: PaginationFilter,
            include: [e => e.Tags]
        );
        controller.MapCrudPost<CmsContext, Article, ArticlePayload>(eventType: CmsEvents.CreateArticle);
        controller.MapCrudPatch<CmsContext, Article>(eventType: CmsEvents.UpdateArticle);
        controller.MapCrudDelete<CmsContext, Article>(eventType: CmsEvents.DeleteArticle);

        var publicController = app
            .MapGroup("cms/articles")
            .WithTags("CMS.Articles")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        publicController
            .MapGet("slug/{slug}", GetArticleBySlug)
            .WithSummary("GetBySlug")
            .WithDescription("Retrieves a published article by its slug.");

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

    private static async
        Task<Results<Ok<Result<ArticleDto>>, NotFound<Result<ArticleDto>>, InternalServerError<Result<ArticleDto>>>>
        GetArticleBySlug(
            string slug,
            ArticleService articleService,
            CancellationToken ct
        )
    {
        var result = await articleService.GetArticleBySlug(slug, ct);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok(result);
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
