using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SafariLib.Core.Environment;

namespace SafariDigital.Database.Context;

public class ContextFactory : IDesignTimeDbContextFactory<SafariDigitalContext>
{
    public SafariDigitalContext CreateDbContext(string[] args)
    {
        var connStr = GetConnectionString(args);
        var optionsBuilder = new DbContextOptionsBuilder<SafariDigitalContext>();
        optionsBuilder.UseNpgsql(connStr);
        return new SafariDigitalContext(optionsBuilder.Options);
    }

    private static string GetConnectionString(string[]? args) =>
        args is not null && args.Length > 0 ? args[0] : GetConnectionString();

    private static string GetConnectionString(string target = "Default") =>
        new ConfigurationBuilder().AddProjectSettings("SafariDigital.Api").Build().GetConnectionString(target)
        ?? throw new InvalidOperationException("No connection string found in appsettings files");
}