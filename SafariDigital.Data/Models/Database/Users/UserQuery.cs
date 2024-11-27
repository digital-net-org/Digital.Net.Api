using Digital.Net.Entities.Models;

namespace SafariDigital.Data.Models.Database.Users;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public EUserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}