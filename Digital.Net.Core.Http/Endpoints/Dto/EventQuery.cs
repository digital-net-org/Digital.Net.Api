using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class EventQuery : Query
{
    public Guid? UserId { get; set; }
    public string? EventType { get; set; }
}
