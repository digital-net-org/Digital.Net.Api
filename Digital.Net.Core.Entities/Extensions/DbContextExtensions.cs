using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Extensions;

public static class DbContextSaveExtensions
{
    /// <summary>
    ///     Saves pending changes; if EF throws a <see cref="DbUpdateException" /> caused by a
    ///     constraint violation that the client can act on (unique / fk / not-null / varchar),
    ///     re-throws as an <see cref="EntityValidationException" /> so the standard error pipeline
    ///     surfaces it as <c>400 Bad Request</c>. Unknown provider codes bubble up unchanged
    ///     and end up as <c>500 Internal Server Error</c>.
    /// </summary>
    public static async Task<int> SaveEntityAsync(
        this DbContext context,
        CancellationToken ct = default
    )
    {
        try
        {
            return await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException due)
        {
            var translated = DbUpdateExceptionTranslator.TryTranslate(due);
            if (translated is not null) throw translated;
            throw;
        }
    }


    public static void MarkDirty<T>(this DbContext context, Guid id)
        where T : Entity
    {
        var entity = context.Set<T>().Find(id);
        if (entity is null) return;
        var entry = context.Entry(entity);
        if (entry.State == EntityState.Unchanged) entry.State = EntityState.Modified;
    }
}