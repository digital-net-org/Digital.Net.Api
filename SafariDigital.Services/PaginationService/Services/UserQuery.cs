using SafariDigital.Database.Models.UserTable;
using SafariDigital.Services.PaginationService.Models;

namespace SafariDigital.Services.PaginationService.Services;

public class UserQuery : PaginationQuery
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public EUserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}