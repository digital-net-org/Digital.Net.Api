using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints;
using Digital.Net.Cms.Models;
using Digital.Net.Cms.Services;
using Digital.Net.Core.Bootstrap;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Crud;
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
            .AddScoped<ICrudValidationService<CmsContext>, CrudValidationService<CmsContext>>()
            .AddScoped<ICrudService<Page>, CrudService<CmsContext, Page>>()
            .AddScoped<ICrudService<Article>, CrudService<CmsContext, Article>>()
            .AddScoped<ICrudService<Sheet>, CrudService<CmsContext, Sheet>>()
            .AddScoped<ICrudService<Tag>, CrudService<CmsContext, Tag>>()
            .AddScoped<ICrudService<Media>, CrudService<CmsContext, Media>>()
            .AddScoped<IMediaService, MediaService>()
            .AddScoped<ICrudService<Form>, CrudService<CmsContext, Form>>()
            .AddScoped<ICrudService<FormField>, CrudService<CmsContext, FormField>>()
            .AddScoped<ICrudService<FormSubmission>, CrudService<CmsContext, FormSubmission>>();

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
            .MapCmsArticleEndpoints()
            .MapCmsMediaEndpoints()
            .MapCmsSitemapEndpoints()
            .MapCmsSheetEndpoints()
            .MapCmsFormEndpoints();

        app
            .MapSseStream(
                "cms/events/stream",
                "mutation",
                signal => signal.Name.StartsWith("CMS_") && signal.State == EventState.Success
            )
            .WithTags("CMS - Events")
            .WithSummary("SSE Stream")
            .WithDescription("Subscribe to CMS mutation events via Server-Sent Events.")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application);

        return app;
    }
}