using Digital.Net.Core.Entities.Models.Events;

namespace Digital.Net.Core.Endpoints.Dto;

public class EventDto
{
    public EventDto() {}

    public EventDto(Event e)
    {
        Id = e.Id;
        Name = e.Name;
        Payload = e.Payload;
        UserId = e.UserId;
        State = e.State;
        HasError = e.HasError;
        ErrorTrace = e.ErrorTrace;
        CreatedAt = e.CreatedAt;
        UpdatedAt = e.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Payload { get; init; }
    public Guid? UserId { get; init; }
    public EventState? State { get; init; }
    public bool HasError { get; init; }
    public string? ErrorTrace { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
