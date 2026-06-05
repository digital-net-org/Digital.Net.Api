using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Http.Services;
using Digital.Net.Core.Http.RateLimiters;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Cms.Http.Endpoints;

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

    private static async Task<IResult> GetSitemapData(SitemapService sitemapService)
    {
        var entries = await sitemapService.GetEntriesAsync();
        return Results.Ok(new Result<List<SitemapEntryDto>>(entries));
    }
}