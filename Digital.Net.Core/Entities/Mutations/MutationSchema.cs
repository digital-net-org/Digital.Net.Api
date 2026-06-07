namespace Digital.Net.Core.Entities.Mutations;

/// <summary>
///     A Postgres schema that owns an <c>EntityMutation</c> table — one is registered per database context (in
///     <c>AddDatabaseContext</c>). Injected as <c>IEnumerable&lt;MutationSchema&gt;</c> so cross-schema readers
///     iterate the schemas without referencing each context type.
/// </summary>
public sealed record MutationSchema(string Name);
