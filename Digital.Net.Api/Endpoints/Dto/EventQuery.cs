using Digital.Net.Entities.Crud.Enpoints;

namespace Digital.Net.Api.Endpoints.Dto;

public class EventQuery : Query
{
    public Guid? UserId { get; set; }
    public string? EventType { get; set; }
}
