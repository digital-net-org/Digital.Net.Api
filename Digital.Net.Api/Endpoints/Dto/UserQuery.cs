using Digital.Net.Entities.Crud.Endpoints;

namespace Digital.Net.Api.Endpoints.Dto;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}