using Digital.Net.Core.Entities.Context;
using Digital.Net.Lib.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Bootstrap;

public static class ContextInjector
{
    public static WebApplicationBuilder AddDatabaseContext<T>(this WebApplicationBuilder builder)
        where T : DbContext
    {
        var connectionString = builder.Configuration.GetOrThrow<string>($"{CoreSettings.ConnectionStringKey}");
        builder.Services.AddDbContext<T>(options => options.UseDigitalNpgsql<T>(connectionString));
        return builder;
    }

    public static WebApplicationBuilder ApplyMigrations<T>(this WebApplicationBuilder builder)
        where T : DbContext
    {
        var context = builder.Services.BuildServiceProvider().GetRequiredService<T>();
        context.Database.Migrate();
        return builder;
    }
}
