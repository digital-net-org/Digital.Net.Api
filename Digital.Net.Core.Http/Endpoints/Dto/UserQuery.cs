using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsAdmin { get; set; }
}