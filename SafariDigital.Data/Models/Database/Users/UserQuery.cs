using Safari.Net.Data.Entities;

namespace SafariDigital.Data.Models.Database.Users;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public EUserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}