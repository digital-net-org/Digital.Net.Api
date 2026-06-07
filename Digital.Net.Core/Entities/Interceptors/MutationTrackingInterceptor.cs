using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Entities.Models.Mutations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Entities.Interceptors;

/// <summary>
///     Captures every mutation of a tracked entity (any <see cref="Entity" /> not marked
///     <see cref="IUntrackedEntity" />) as an <see cref="EntityMutation" /> row, written in the same
///     transaction as the source change.
/// </summary>
public class MutationTrackingInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Capture(DbContext? context)
    {
        if (context is null) return;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e =>
                e.Entity is Entity and not IUntrackedEntity
                && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
            )
            .ToList();
        if (entries.Count == 0) return;

        var origin = serviceProvider.GetRequiredService<IOriginAccessor>().TryGetOrigin();
        var userId = serviceProvider.GetRequiredService<IUserAccessor>().TryGetUserId();
        var now = DateTime.UtcNow;

        var mutations = entries.Select(entry =>
        {
            var entity = (Entity)entry.Entity;
            return new EntityMutation
            {
                ChangeType = entry.State switch
                {
                    EntityState.Added => ChangeType.Created,
                    EntityState.Modified => ChangeType.Updated,
                    _ => ChangeType.Deleted
                },
                EntityType = entity.GetCanonicalType().Name,
                EntityId = entity.Id,
                UserId = userId,
                IpAddress = origin?.IpAddress,
                UserAgent = origin?.UserAgent,
                CreatedAt = now
            };
        }).ToList();

        context.AddRange(mutations);
    }
}