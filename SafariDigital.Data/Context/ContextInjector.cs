using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Core.Application;

namespace SafariDigital.Data.Context;

public static class ContextInjector
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<SafariDigitalContext>(opts =>
        {
            opts.UseLazyLoadingProxies();

            if (ApplicationEnvironment.IsTest)
                opts.UseSqlite(new SqliteConnection("Filename=:memory:"));
            else
                opts.UseNpgsql(builder.GetConnectionString(), b => b.MigrationsAssembly("SafariDigital.Data"));
        }, ServiceLifetime.Transient);
        return builder;
    }

    private static string GetConnectionString(this WebApplicationBuilder builder) =>
        builder.Configuration.GetConnectionString("Default") ??
        throw new InvalidOperationException("Postgres connection string is not set");
}