using Digital.Lib.Net.Entities.Context;
using Microsoft.EntityFrameworkCore.Design;

namespace Digital.Pages.Api.Data;

public class ContextFactory : IDesignTimeDbContextFactory<DigitalPagesContext>
{
    public DigitalPagesContext CreateDbContext(string[] args)
    {
        var opts = ContextBuilder.GetConnectionString(args);
        return ContextBuilder.Build<DigitalPagesContext>(opts);
    }
}