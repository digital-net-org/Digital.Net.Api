using Digital.Net.Api.Core.Extensions.ConfigurationUtilities;
using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Entities.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Sdk.Bootstrap;

public static class ContextInjector
{
    public static WebApplicationBuilder AddDatabaseContext<T>(this WebApplicationBuilder builder)
        where T : DbContext
    {
        var connectionString = builder.Configuration.GetOrThrow<string>($"{ApplicationSettingsAccessor.ConnectionString}");
        var useSqlite = builder.Configuration.Get<bool>(ApplicationSettingsAccessor.UseSqlite);

        builder.Services.AddDbContext<T>(options =>
        {
            if (useSqlite)
                options.UseSqlite(connectionString);
            else
                options.UseDigitalNpgsql<T>(connectionString);
        });

        if (useSqlite)
        {
            var context = builder.Services.BuildServiceProvider().GetService<T>();
            context?.Database.EnsureCreated();
        }

        return builder;
    }

    public static WebApplicationBuilder ApplyMigrations<T>(this WebApplicationBuilder builder)
        where T : DbContext
    {
        if (builder.Configuration.Get<bool>(ApplicationSettingsAccessor.UseSqlite))
            return builder;

        var context = builder.Services.BuildServiceProvider().GetRequiredService<T>();
        context.Database.Migrate();
        return builder;
    }
}