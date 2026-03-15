using Digital.Net.Core.Services.Pagination;

namespace Digital.Net.Core.Endpoints.Dto;

public class EventQuery : Query
{
    public Guid? UserId { get; set; }
    public string? EventType { get; set; }
}
