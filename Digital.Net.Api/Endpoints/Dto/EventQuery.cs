using Digital.Net.Entities.Crud.Endpoints;

namespace Digital.Net.Api.Endpoints.Dto;

public class EventQuery : Query
{
    public Guid? UserId { get; set; }
    public string? EventType { get; set; }
}
