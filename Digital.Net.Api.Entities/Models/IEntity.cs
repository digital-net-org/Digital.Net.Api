namespace Digital.Net.Api.Entities.Models;

public interface IEntity
{
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
