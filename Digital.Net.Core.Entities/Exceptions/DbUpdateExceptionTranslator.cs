using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Exceptions;

/// <summary>
///     Translates a <see cref="DbUpdateException" /> raised by EF Core into an
///     <see cref="EntityValidationException" /> with a client-readable message, when the inner
///     provider exception describes a constraint violation that the client can act on
///     (unique, foreign key, not-null, varchar overflow). Returns <c>null</c> for unknown
///     provider codes — the caller must rethrow so the error surfaces as 500.
/// </summary>
public static class DbUpdateExceptionTranslator
{
    public static EntityValidationException? TryTranslate(DbUpdateException ex)
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
