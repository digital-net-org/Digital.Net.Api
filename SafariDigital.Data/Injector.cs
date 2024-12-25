using Digital.Net.Core.Application;
using Digital.Net.Core.Environment;
using Digital.Net.Database;
using Digital.Net.Database.Options;
using Digital.Net.Entities;
using Microsoft.AspNetCore.Builder;
using SafariDigital.Data.Context;

namespace SafariDigital.Data;

public static class Injector
{
    public static WebApplicationBuilder AddSafariDigitalDatabase(this WebApplicationBuilder builder)
    {
        builder.AddDbConnector<SafariDigitalContext>(options =>
        {
            options.SetDatabaseEngine(DatabaseEngine.PostgreSql);
            options.SetMigrationAssembly("SafariDigital.Data");

            if (!AspNetEnv.IsTest)
                options.SetConnectionString(builder.GetConnectionString());
        });
        builder.Services.AddDigitalEntities<SafariDigitalContext>();
        return builder;
    }
}