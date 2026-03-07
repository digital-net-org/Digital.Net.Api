using Digital.Net.Entities.Crud.Controllers;

namespace Digital.Net.Controllers.Dto;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}