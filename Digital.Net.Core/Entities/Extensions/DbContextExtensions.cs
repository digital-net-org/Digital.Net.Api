using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Extensions;

public static class DbContextSaveExtensions
{
    /// <summary>
    ///     Saves pending changes; if EF throws a <see cref="DbUpdateException" /> caused by a
    ///     constraint violation that the client can act on (unique / fk / not-null / varchar),
    ///     re-throws as an <see cref="EntityValidationException" />. Unknown provider codes bubble up unchanged.
    /// </summary>
    public static async Task SaveEntityAsync(this DbContext context, CancellationToken ct = default)
    {
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException due)
        {
            var translated = TryTranslate(due);
            if (translated is not null) throw translated;
            throw;
        }
    }


    /// <summary>
    ///     Marks an existing entity as modified so that Entity Framework includes it in the next save operation.
    ///     Entities already tracked with a state other than <see cref="EntityState.Unchanged" /> are left unchanged.
    /// </summary>
    public static void MarkDirty<T>(this DbContext context, Guid id)
        where T : Entity
    {
        var entity = context.Set<T>().Find(id);
        if (entity is null) return;
        var entry = context.Entry(entity);
        if (entry.State == EntityState.Unchanged) entry.State = EntityState.Modified;
    }

    private static EntityValidationException? TryTranslate(DbUpdateException ex)
    {
        var inner = ex.InnerException;
        if (inner is null) return null;

        return inner.GetType().FullName switch
        {
            "Npgsql.PostgresException" => TranslatePostgres(inner),
            _ => null
        };
    }

    private static EntityValidationException? TranslatePostgres(Exception inner)
    {
        var sqlState = (string?)inner.GetType().GetProperty("SqlState")?.GetValue(inner);
        var columnName = (string?)inner.GetType().GetProperty("ColumnName")?.GetValue(inner);
        var constraintName = (string?)inner.GetType().GetProperty("ConstraintName")?.GetValue(inner);
        var label = columnName ?? ExtractColumnFromConstraint(constraintName);

        return sqlState switch
        {
            "23505" => new EntityValidationException($"{label}: This value violates a unique constraint."),
            "23503" => new EntityValidationException($"{label}: Referenced entity does not exist."),
            "23502" => new EntityValidationException($"{label}: This field is required and cannot be null."),
            "22001" => new EntityValidationException($"{label}: Maximum length exceeded."),
            _ => null
        };
    }

    private static string ExtractColumnFromConstraint(string? constraintName) =>
        constraintName?.Split('_').LastOrDefault() ?? "field";
}