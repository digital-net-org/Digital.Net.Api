using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Digital.Net.Api.Entities.Context;

public abstract class DesignTimeContextFactory<T> : IDesignTimeDbContextFactory<T>
    where T : DbContext
{
    
    public T CreateDbContext(string[] args) => Build(GetConnectionString(args));
    
    private static T Build(string connStr)
    {
        var optionsBuilder = new DbContextOptionsBuilder<T>();
        optionsBuilder.UseDigitalNpgsql<T>(connStr);
        return (T)Activator.CreateInstance(typeof(T), optionsBuilder.Options)!;
    }

    public static string GetConnectionString(string?[]? args)
    {
        var result = args is not null && args.Length > 0 ? args[0] : null;
        return result ?? throw new Exception("No connection string specified in args.");
    }
}