namespace Digital.Net.Lib.Entities.Mutations;

public sealed record MutationSignal(
    ChangeType ChangeType,
    string EntityType,
    Guid EntityId,
    DateTime CreatedAt,
    Guid Id,
    Guid? UserId = null,
    string? OriginClientId = null
);