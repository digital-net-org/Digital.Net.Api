using SafariDigital.Data.Models.Database.Users;

namespace SafariDigital.Services.Jwt.Models;

public class AuthenticatedUser
{
    public Guid? Id { get; init; }
    public EUserRole? Role { get; init; }

    public string? Token { get; set; } = null;
}