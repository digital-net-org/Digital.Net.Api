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
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Endpoints;

public static class ArticleEndpoints
{
    public static IEndpointRouteBuilder MapCmsArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("cms/articles")
            .WithTags("CMS - Articles")
            .RequireRateLimiting(GlobalLimiter.Policy);

        controller
            .MapGet("path/{*path}", GetArticleByPath)
            .WithSummary("GetByPath")
            .WithDescription("Retrieves a published article by its path. Requires Application authentication.")
            .RequireAuthentication(AuthorizeType.Application);

        controller
            .MapCrudSchema<CmsContext, Article>("")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapCrudGet<Article, ArticleDto>("")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapPaginationGet<CmsContext, Article, ArticleDto, ArticleQuery>("", PaginationFilter)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapPost("", CreateArticle)
            .WithSummary("Create")
            .WithDescription("Creates a new article.")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapPatch("{id:guid}", UpdateArticle)
            .WithSummary("Patch")
            .WithDescription("Updates an article by its ID.")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapDelete("{id:guid}", DeleteArticle)
            .WithSummary("Delete")
            .WithDescription("Deletes an article and its associated page by its ID.")
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

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

        return article is null ? TypedResults.NotFound() : TypedResults.Ok(new Result<ArticleDto>(new ArticleDto(article)));
    }

    private static async Task<IResult> CreateArticle(
        [FromBody]
        ArticlePayload payload,
        ICrudService<Article> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var entity = new Article
        {
            Path = payload.Path,
            Name = payload.Name,
            Content = payload.Content
        };
        var result = await crudService.Create(entity);
        await auditService.RegisterEventAsync(
            CmsEvents.CreateArticle,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.BadRequest(result)
            : Results.Ok(result);
    }

    private static async Task<IResult> UpdateArticle(
        Guid id,
        [FromBody]
        JsonElement patch,
        ICrudService<Article> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await crudService.Patch(patch.GetPatchDocument<Article>(), id);
        await auditService.RegisterEventAsync(
            CmsEvents.UpdateArticle,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.BadRequest(result)
            : Results.Ok(result);
    }

    private static async Task<IResult> DeleteArticle(
        Guid id,
        ICrudService<Article> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await crudService.Delete(id);
        await auditService.RegisterEventAsync(
            CmsEvents.DeleteArticle,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.NotFound(result)
            : Results.Ok(result);
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