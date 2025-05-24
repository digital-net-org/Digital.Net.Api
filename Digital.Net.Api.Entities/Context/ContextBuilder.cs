using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Context;

public static class ContextBuilder
{
    public const string EfCoreSchema = "ef_core_history";

    public static DbContextOptionsBuilder UseDigitalNpgsql<T>(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString
    )
        where T : DbContext
    {
        optionsBuilder
            .UseLazyLoadingProxies()
            .UseNpgsql(
                connectionString,
                o => o.MigrationsHistoryTable($"{typeof(T).Name}Migration", EfCoreSchema)
            );
        return optionsBuilder;
    }
}