namespace Digital.Net.Core.Http.Services.Mutations;

public class MutationRow
{
    public int ChangeType { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid Id { get; set; }
}