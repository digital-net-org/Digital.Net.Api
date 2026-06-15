using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Lib.Entities.Context;

public static class ContextBuilder
{
    public const string EfCoreSchema = "ef_core_history";

    public static DbContextOptionsBuilder UseDigitalNpgsql<T>(
        this DbContextOptionsBuilder builder,
        string connectionString
    )
        where T : DbContext
    {
        builder
            .UseLazyLoadingProxies()
            .UseNpgsql(
                connectionString,
                o => o.MigrationsHistoryTable($"{typeof(T).Name}Migration", EfCoreSchema)
            );
        return builder;
    }
}