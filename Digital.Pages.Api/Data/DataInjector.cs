using Digital.Lib.Net.Entities;
using Digital.Pages.Api.Data.Frames;
using Digital.Pages.Api.Data.Views;

namespace Digital.Pages.Api.Data;

public static class DataInjector
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDigitalContext()
            .AddScoped<DigitalPagesContext>()
            .AddDigitalEntities<Frame>()
            .AddDigitalEntities<View>();

        return builder;
    }
}