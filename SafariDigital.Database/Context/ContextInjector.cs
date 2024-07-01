using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SafariDigital.Database.Context;

public static class ContextInjector
{
    public static WebApplicationBuilder ConnectDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<SafariDigitalContext>(opts =>
            opts.UseNpgsql(builder.GetConnectionString(), b
                => b.MigrationsAssembly("SafariDigital.Database")));
        return builder;
    }

    private static string GetConnectionString(this WebApplicationBuilder builder) =>
        builder.Configuration.GetConnectionString("Default") ??
        throw new InvalidOperationException("Postgres connection string is not set");
}