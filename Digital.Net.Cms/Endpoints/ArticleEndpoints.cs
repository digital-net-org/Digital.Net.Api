using System.Linq.Expressions;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Pagination.Extensions;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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

        controller.MapCrudSchema<CmsContext, Article>();
        controller.MapCrudGet<CmsContext, Article, ArticleDto>();
        controller.MapPaginationGet<CmsContext, Article, ArticleDto, ArticleQuery>(filter: PaginationFilter);
        controller.MapCrudPost<CmsContext, Article, ArticlePayload>(eventType: CmsEvents.CreateArticle);
        controller.MapCrudPatch<CmsContext, Article>(eventType: CmsEvents.UpdateArticle);
        controller.MapCrudDelete<CmsContext, Article>(eventType: CmsEvents.DeleteArticle);

        var publicController = app
            .MapGroup("cms/articles")
            .WithTags("CMS.Articles")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        publicController
            .MapGet("path/{*path}", GetArticleByPath)
            .WithSummary("GetByPath")
            .WithDescription("Retrieves a published article by its path. Requires Application authentication.");

        return app;
    }

    private static async Task<Results<Ok<Result<ArticleDto>>, NotFound>> GetArticleByPath(
        string path,
        CmsContext context
    )
    {
        var article = await context.Articles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Path == path && a.Published);

        return article is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new Result<ArticleDto>(new ArticleDto(article)));
    }

    private static Expression<Func<Article, bool>> PaginationFilter(
        Expression<Func<Article, bool>> predicate,
        ArticleQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => x.Name.StartsWith(query.Name));
        if (query.Published.HasValue)
            predicate = predicate.Add(x => x.Published == query.Published);
        if (query.TagId.HasValue)
            predicate = predicate.Add(x => x.Tags.Any(t => t.Id == query.TagId));
        return predicate;
    }
}