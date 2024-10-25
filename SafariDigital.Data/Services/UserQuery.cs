using Safari.Net.Data.Entities;
using SafariDigital.Data.Models.Database.Users;

namespace SafariDigital.Data.Services;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public EUserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}