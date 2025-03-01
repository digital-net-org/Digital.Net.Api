using Digital.Lib.Net.Entities;
using Digital.Lib.Net.Entities.Context;
using Digital.Pages.Api.Data.Frames;
using Digital.Pages.Api.Data.Views;

namespace Digital.Pages.Api.Data;

public static class DataInjector
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder
            .AddDigitalContext()
            .AddNpgsqlContext<DigitalPagesContext>();
        builder.Services
            .AddDigitalEntities<Frame>()
            .AddDigitalEntities<View>();
        return builder;
    }
}