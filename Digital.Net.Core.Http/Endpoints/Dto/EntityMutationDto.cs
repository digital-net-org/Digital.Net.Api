using Digital.Net.Lib.Entities.Mutations;
using Digital.Net.Core.Http.Services.Mutations;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class EntityMutationDto
{
    public EntityMutationDto()
    {
    }

    public EntityMutationDto(MutationAuditRow row, bool includeAuthorOrigin)
    {
        Id = row.Id;
        ChangeType = (ChangeType)row.ChangeType;
        EntityType = row.EntityType;
        EntityId = row.EntityId;
        Payload = row.Payload;
        UserId = row.UserId;
        CreatedAt = row.CreatedAt;
        UpdatedAt = row.UpdatedAt;

        // Author origin is admin-only
        if (!includeAuthorOrigin) return;
        IpAddress = row.IpAddress;
        UserAgent = row.UserAgent;
    }

    public Guid Id { get; init; }
    public ChangeType ChangeType { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public Guid? UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Payload { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}