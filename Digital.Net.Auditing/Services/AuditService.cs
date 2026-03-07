using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.Events;
using Digital.Net.Entities.Context;

namespace Digital.Net.Auditing.Services;

public class AuditService(DigitalContext context) : IAuditService
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
        
        await context.Events.AddAsync(appEvent);
        await context.SaveChangesAsync();
    }
}