using Digital.Net.Core.Services.Pagination;

namespace Digital.Net.Core.Endpoints.Dto;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}