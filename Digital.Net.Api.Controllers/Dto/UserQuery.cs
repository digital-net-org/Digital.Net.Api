using Digital.Net.Api.Entities.Crud.Controllers;

namespace Digital.Net.Api.Controllers.Dto;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}