using Digital.Lib.Net.Core.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Digital.Pages.Api.Data;

public class ContextFactory : IDesignTimeDbContextFactory<DigitalPagesContext>
{
    public DigitalPagesContext CreateDbContext(string[] args)
    {
        var connStr = ApplicationSettings.GetConnectionString(args);
        var optionsBuilder = new DbContextOptionsBuilder<DigitalPagesContext>();
        optionsBuilder.UseNpgsql(connStr);
        return new DigitalPagesContext(optionsBuilder.Options);
    }
}