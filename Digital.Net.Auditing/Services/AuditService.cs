using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.Events;
using Digital.Net.Entities.Repositories;

namespace Digital.Net.Auditing.Services;

public class AuditService(IRepository<Event> eventRepository) : IAuditService
{
    public async Task RegisterEventAsync(
        string name,
        EventState state,
        Result? result,
        Guid? userId,
        string? payload = null,
        string? userAgent = null,
        string? ipAddress = null
    )
    {
        var appEvent = new Event
        {
            Name = name,
            State = state,
            UserId = userId,
            Payload = payload,
            UserAgent = userAgent ?? string.Empty,
            IpAddress = ipAddress ?? string.Empty
        };
        
        if (result is not null && result.HasError)
            appEvent.SetError(result);
        
        await eventRepository.CreateAndSaveAsync(appEvent);
    }
}