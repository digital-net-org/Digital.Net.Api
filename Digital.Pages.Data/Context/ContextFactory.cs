using Digital.Lib.Net.Core.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Digital.Pages.Data.Context;

public class ContextFactory : IDesignTimeDbContextFactory<DigitalContext>
{
    public DigitalContext CreateDbContext(string[] args)
    {
        var connStr = ApplicationSettings.GetConnectionString(args)
                      ?? ApplicationSettings.GetExternalConnectionString("Digital.Pages.Api");
        var optionsBuilder = new DbContextOptionsBuilder<DigitalContext>();
        optionsBuilder.UseNpgsql(connStr);
        return new DigitalContext(optionsBuilder.Options);
    }
}