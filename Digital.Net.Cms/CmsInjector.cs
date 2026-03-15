using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints;
using Digital.Net.Cms.Models;
using Digital.Net.Core.Bootstrap;
using Digital.Net.Core.Services.Crud;
using Microsoft.AspNetCore.Builder;
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
            .AddScoped<ICrudService<Tag>, CrudService<CmsContext, Tag>>();

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
            .MapCmsSitemapEndpoints();

        return app;
    }
}