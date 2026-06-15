namespace Digital.Net.Core.Http.Endpoints.Dto;

public class UserListDto
{
    public UserListDto()
    {
    }

    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Login { get; init; }
    public string? Email { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsAdmin { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}