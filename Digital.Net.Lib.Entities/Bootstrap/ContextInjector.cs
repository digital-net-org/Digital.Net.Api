using Digital.Net.Lib.Entities.Context;
using Digital.Net.Lib.Entities.Interceptors;
using Digital.Net.Lib.Entities.Models;
using Digital.Net.Lib.Entities.Mutations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Digital.Net.Lib.Entities.Bootstrap;

public static class ContextInjector
{
    public static IHostApplicationBuilder AddDatabaseContext<T>(
        this IHostApplicationBuilder builder,
        string connectionString
    ) where T : DbContext, IDigitalNetContext
    {
        builder.Services.AddSingleton(new MutationSchema(T.Schema));
        builder.Services.AddSingleton(new MigratableContext(typeof(T)));
        RegisterAuditedEntityTypes<T>(builder.Services);

        builder.Services.TryAddSingleton<MutationBroadcaster>();
        builder.Services.TryAddScoped<MutationTrackingInterceptor>();

        builder.Services.AddDbContext<T>((sp, options) =>
            options
                .UseDigitalNpgsql<T>(connectionString)
                .AddInterceptors(sp.GetRequiredService<MutationTrackingInterceptor>())
        );

        return builder;
    }

    public static async Task ApplyDigitalNetMigrationsAsync(this IHost app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        foreach (var migratable in scope.ServiceProvider.GetServices<MigratableContext>())
            await ((DbContext)scope.ServiceProvider.GetRequiredService(migratable.ContextType))
                .Database.MigrateAsync();
    }

    private static void RegisterAuditedEntityTypes<T>(IServiceCollection services) where T : DbContext
    {
        var entityTypes = typeof(T).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && typeof(Entity).IsAssignableFrom(type)
                           && !typeof(IUntrackedEntity).IsAssignableFrom(type));

        foreach (var type in entityTypes)
            services.AddSingleton(
                new AuditedEntityType(type.Name, typeof(IRestrictedAuditEntity).IsAssignableFrom(type))
            );
    }
}