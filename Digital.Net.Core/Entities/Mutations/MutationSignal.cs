using Digital.Net.Core.Entities.Models.Mutations;

namespace Digital.Net.Core.Entities.Mutations;

public sealed record MutationSignal(
    ChangeType ChangeType,
    string EntityType,
    Guid EntityId,
    DateTime CreatedAt,
    Guid Id
);