using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints;
using Digital.Net.Cms.Services.Medias;
using Digital.Net.Cms.Services.Pages;
using Digital.Net.Cms.Services.Sitemaps;
using Digital.Net.Core.Bootstrap;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Pivots;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Events.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Cms;

public static class CmsInjector
{
    /// <summary>
    ///     Add the optional Digital.Net CMS module to the application.
    /// </summary>
    public static WebApplicationBuilder AddDigitalCms(this WebApplicationBuilder builder)
    {
        builder
            .AddDatabaseContext<CmsContext>()
            .ApplyMigrations<CmsContext>();

        builder.Services
            .AddPageDependencies()
            .AddSitemapDependencies()
            .AddScoped<MediaService>()
            .AddPivotsFromAssemblies<CmsContext>(typeof(CmsInjector).Assembly);

        return builder;
    }

    /// <summary>
    ///     Use the optional Digital.Net CMS module.
    /// </summary>
    public static WebApplication UseDigitalCms(this WebApplication app)
    {
        app
            .MapCmsTagEndpoints()
            .MapCmsPageEndpoints()
            .MapCmsPagePublicEndpoints()
            .MapCmsArticleEndpoints()
            .MapCmsMediaEndpoints()
            .MapCmsSitemapEndpoints()
            .MapCmsFormEndpoints();

        app
            .MapSseStream(
                "cms/events/stream",
                "mutation",
                signal => signal.Name.StartsWith("CMS_") && signal.State == EventState.Success
            )
            .WithTags("CMS.Events")
            .WithSummary("SSE Stream")
            .WithDescription("Subscribe to CMS mutation events via Server-Sent Events.")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        return app;
    }
}