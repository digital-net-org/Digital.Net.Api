using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Database.Context;

namespace Tests.Core.Integration;

public class ApiFactory<T> : WebApplicationFactory<T>
    where T : class
{
    private readonly SqliteConnection _connection;

    public ApiFactory()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.UseConfiguration(CreateConfigurationBuilder());
        builder.ConfigureTestServices(s => { AddMemoryDatabase(s, _connection); });
    }

    private static IConfigurationRoot CreateConfigurationBuilder()
    {
        var config = new ConfigurationBuilder();
        return config.Build();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }

    public static void AddMemoryDatabase(IServiceCollection services, SqliteConnection connection)
    {
        RemoveDbContext<SafariDigitalContext>(services);
        CreateDbContext<SafariDigitalContext>(services, connection);
        services.BuildServiceProvider().GetService<SafariDigitalContext>()?.Database.EnsureCreated();
    }

    private static void RemoveDbContext<TC>(IServiceCollection services)
        where TC : DbContext
    {
        var dbContextDescriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(DbContextOptions<TC>)
        );
        var dbConnectionDescriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(DbConnection)
        );
        if (dbContextDescriptor is not null) services.Remove(dbContextDescriptor);
        if (dbConnectionDescriptor is not null) services.Remove(dbConnectionDescriptor);
    }

    private static void CreateDbContext<TC>(IServiceCollection services, SqliteConnection connection)
        where TC : DbContext =>
        services
            .AddEntityFrameworkSqlite()
            .AddDbContext<TC>(
                options =>
                {
                    options.UseSqlite(connection);
                    options.UseInternalServiceProvider(services.BuildServiceProvider());
                },
                ServiceLifetime.Singleton
            );
}