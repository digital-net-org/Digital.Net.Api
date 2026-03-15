using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Core.Services.Auditing;

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