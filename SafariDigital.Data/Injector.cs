using Digital.Net.Core.Application;
using Digital.Net.Core.Environment;
using Digital.Net.Database.Services;
using Digital.Net.Database.Utils;
using Digital.Net.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Data.Context;

namespace SafariDigital.Data;

public static class Injector
{
    public static WebApplicationBuilder AddSafariDigitalDatabase(this WebApplicationBuilder builder)
    {
        // builder.AddDbConnector<SafariDigitalContext>(options =>
        // {
        //     options.SetDatabaseEngine(DatabaseEngine.PostgreSql);
        //     options.SetMigrationAssembly("SafariDigital.Data");
        //
        //     if (!AspNetEnv.IsTest)
        //         options.SetConnectionString(builder.GetConnectionString());
        // });

        builder.Services.AddDbContext<SafariDigitalContext>((provider, opts) =>
        {
            if (AspNetEnv.IsTest)
                opts.UseSqlite(DatabaseUtils.InMemorySqliteConnection);
            else
                opts.UseNpgsql(builder.GetConnectionString(), b => b.MigrationsAssembly("SafariDigital.Data"));

            opts.UseLazyLoadingProxies();
        }, ServiceLifetime.Transient);
        builder.Services.AddScoped<IDataAccessor, DataAccessor>();
        builder.Services.AddDigitalEntities<SafariDigitalContext>();
        return builder;
    }
}