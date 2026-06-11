using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Interceptors;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Lib.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Digital.Net.Core.Bootstrap;

public static class ContextInjector
{
    public static IHostApplicationBuilder AddDatabaseContext<T>(this IHostApplicationBuilder builder)
        where T : DbContext, ISchemaContext
    {
        var connectionString = builder.Configuration.GetOrThrow<string>($"{CoreSettings.ConnectionStringKey}");
        builder.Services.AddSingleton(new MutationSchema(T.Schema));
        RegisterAuditedEntityTypes<T>(builder.Services);
        builder.Services.TryAddSingleton<MutationBroadcaster>();
        builder.Services.TryAddScoped<MutationTrackingInterceptor>();
        builder.Services.AddDbContext<T>((sp, options) =>
            options
                .UseDigitalNpgsql<T>(connectionString)
                .AddInterceptors(sp.GetRequiredService<MutationTrackingInterceptor>()));
        return builder;
    }

    public static IHostApplicationBuilder ApplyMigrations<T>(this IHostApplicationBuilder builder)
        where T : DbContext
    {
        var context = builder.Services.BuildServiceProvider().GetRequiredService<T>();
        context.Database.Migrate();
        return builder;
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
