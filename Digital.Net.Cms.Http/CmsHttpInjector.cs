using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Endpoints;
using Digital.Net.Cms.Http.Services;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Pivots;
using Digital.Net.Core.Http.RateLimiters;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Core.Http.Services.Events.Extensions;
using Digital.Net.Lib.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Cms.Http;

public static class CmsHttpInjector
{
    /// <summary>
    ///     Registers all the Digital.Net CMS HTTP adapters and CRUD-coupled services. The CMS business layer must
    ///     be registered separately via <c>AddDigitalNetCms()</c>.
    /// </summary>
    public static WebApplicationBuilder AddDigitalNetCmsHttp(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<PagePublicService>()
            .AddScoped<ArticleService>()
            .AddScoped<MediaLabelService>()
            .AddScoped<SitemapService>()
            .AddScoped<PageCrudService>()
            .AddEntitiesPivots<CmsContext>(typeof(CmsInjector).Assembly, typeof(CmsHttpInjector).Assembly)
            .AddDtoEnrichersFromAssemblies(typeof(CmsHttpInjector).Assembly);

        builder.Services
            .RequireContract<CmsContext>("AddDigitalNetCms");

        return builder;
    }

    /// <summary>
    ///     Wires the Digital.Net CMS HTTP pipeline.
    /// </summary>
    public static WebApplication UseDigitalNetCmsHttp(this WebApplication app)
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