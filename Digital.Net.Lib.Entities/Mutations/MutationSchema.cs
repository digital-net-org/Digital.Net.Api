namespace Digital.Net.Lib.Entities.Mutations;

/// <summary>
///     A Postgres schema that owns an <c>EntityMutation</c> table. Used by cross-schema readers that iterates the
///     schemas without referencing each context type.
/// </summary>
public sealed record MutationSchema(string Name);