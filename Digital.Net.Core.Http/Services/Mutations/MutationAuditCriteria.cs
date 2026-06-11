namespace Digital.Net.Core.Http.Services.Mutations;

public sealed record MutationAuditCriteria
{
    public int Index { get; init; } = 1;
    public int Size { get; init; } = 50;
    public bool IncludeRestricted { get; init; }
    public string? OrderBy { get; init; }
    public bool Descending { get; init; } = true;
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public Guid? UserId { get; init; }
    public int? ChangeType { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
}

public sealed record MutationAuditPage(IReadOnlyList<MutationAuditRow> Rows, int Total);