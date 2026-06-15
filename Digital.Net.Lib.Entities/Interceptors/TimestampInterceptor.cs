using Digital.Net.Lib.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Digital.Net.Lib.Entities.Interceptors;

public class TimestampInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateTimestamps(DbContext? context)
    {
        if (context == null) return;

        var now = DateTime.UtcNow;
        // Every Entity carries CreatedAt/UpdatedAt and must be stamped (including IUntrackedEntity) never out of
        // timestamping: skipping them here left CreatedAt = default, silently breaking the login lockout window
        // and oldest-session revocation.
        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is Entity &&
                        e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
            switch (entry.State)
            {
                case EntityState.Added:
                {
                    var createdAt = entry.Property("CreatedAt");
                    if (createdAt.CurrentValue is not DateTime value || value == default)
                        createdAt.CurrentValue = now;
                    break;
                }
                case EntityState.Modified:
                    entry.Property("UpdatedAt").CurrentValue = now;
                    break;
            }
    }
}
