using Digital.Lib.Net.Core.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SafariDigital.Data.Context;

public class ContextFactory : IDesignTimeDbContextFactory<SafariDigitalContext>
{
    public SafariDigitalContext CreateDbContext(string[] args)
    {
        var connStr = ApplicationSettings.GetConnectionString(args)
                      ?? ApplicationSettings.GetExternalConnectionString("SafariDigital.Api");
        var optionsBuilder = new DbContextOptionsBuilder<SafariDigitalContext>();
        optionsBuilder.UseNpgsql(connStr);
        return new SafariDigitalContext(optionsBuilder.Options);
    }
}