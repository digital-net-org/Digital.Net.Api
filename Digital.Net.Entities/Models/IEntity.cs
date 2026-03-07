namespace Digital.Net.Entities.Models;

public interface IEntity
{
    public Guid Id { get; init; }
    
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
