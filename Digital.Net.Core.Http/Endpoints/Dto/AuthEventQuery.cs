using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class AuthEventQuery : Query
{
    public AuthEventQuery()
    {
        Order = "desc";
    }

    public AuthEventType? Type { get; set; }
    public bool? Success { get; set; }
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
}