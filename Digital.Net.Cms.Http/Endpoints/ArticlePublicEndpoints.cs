using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Http.Services;
using Digital.Net.Core.Http.Security;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Pagination;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Cms.Http.Endpoints;

public static class ArticlePublicEndpoints
{
    public static IEndpointRouteBuilder MapCmsArticlePublicEndpoints(this IEndpointRouteBuilder app)
    {
        var publicController = app
            .MapGroup("cms/articles/public")
            .WithTags("CMS.Articles.Public")
            .RequireRateLimiting(RateLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        publicController
            .MapGet("", GetPublishedArticles)
            .WithSummary("GetPaginated")
            .WithDescription(
                "Retrieves a paginated list of published articles. " +
                "Ordered by CreatedAt ascending by default; pass OrderBy/Order to customize."
            );

        publicController
            .MapGet("slug/{slug}", GetArticleBySlug)
            .WithSummary("GetBySlug")
            .WithDescription("Retrieves a published article by its slug.");

        return app;
    }

    private static async
        Task<Results<
            Ok<QueryResult<ArticlePublicListDto>>,
            BadRequest<QueryResult<ArticlePublicListDto>>
        >>
        GetPublishedArticles(
            [AsParameters]
            ArticlePublicQuery query,
            ArticleService articleService,
            CancellationToken ct
        )
    {
        var result = await articleService.GetPublishedArticles(query, ct);
        return result.HasErrorOfType<InvalidOrderByException>()
            ? TypedResults.BadRequest(result)
            : TypedResults.Ok(result);
    }

    private static async
        Task<Results<
            Ok<Result<ArticlePublicDto>>,
            NotFound<Result<ArticlePublicDto>>,
            InternalServerError<Result<ArticlePublicDto>>
        >>
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
}