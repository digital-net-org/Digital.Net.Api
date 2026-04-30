using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Endpoints;

public static class SitemapEndpoints
{
    public static IEndpointRouteBuilder MapCmsSitemapEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("cms/sitemaps")
            .WithTags("CMS.Sitemaps")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapGet("data", GetSitemapData)
            .WithSummary("GetSitemapData")
            .WithDescription("Returns all published and indexed pages and articles for sitemap generation.");

        return app;
    }

    private static async Task<IResult> GetSitemapData(CmsContext context)
    {
        var entries = await context.Pages
            .Where(p => p.Published && p.Indexed)
            .Select(p => new SitemapEntryDto
            {
                Path = p.Path,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return Results.Ok(new Result<List<SitemapEntryDto>>(entries));
    }
}