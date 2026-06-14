using Digital.Net.Cms.Context;
using Digital.Net.Cms.Services;
using Digital.Net.Core.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Digital.Net.Cms;

public static class CmsInjector
{
    /// <summary>
    ///     Registers the Digital.Net CMS business layer (CmsContext, migrations, domain services).
    ///     HTTP wiring lives in Digital.Net.Cms.Http (AddDigitalNetCmsHttp / UseDigitalNetCmsHttp).
    /// </summary>
    public static TBuilder AddDigitalNetCms<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder
            .AddDatabaseContext<CmsContext>();

        builder.Services
            .AddScoped<MediaService>();

        return builder;
    }
}
