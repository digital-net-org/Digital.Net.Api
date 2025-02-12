using Digital.Lib.Net.Core.Application;
using Digital.Lib.Net.Core.Environment;
using Digital.Lib.Net.Database;
using Digital.Lib.Net.Database.Options;
using Digital.Lib.Net.Entities;
using Digital.Pages.Data.Context;
using Microsoft.AspNetCore.Builder;

namespace Digital.Pages.Data;

public static class Injector
{
    public static WebApplicationBuilder AddDigitalDatabase(this WebApplicationBuilder builder)
    {
        builder.AddDbConnector<DigitalContext>(options =>
        {
            options.SetDatabaseEngine(DatabaseEngine.PostgreSql);
            options.SetMigrationAssembly("Digital.Pages.Data");

            if (!AspNetEnv.IsTest)
                options.SetConnectionString(builder.GetConnectionString());
        });
        builder.Services.AddDigitalEntities<DigitalContext>();
        return builder;
    }
}