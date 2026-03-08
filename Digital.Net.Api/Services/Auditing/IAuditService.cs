using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.Events;

namespace Digital.Net.Api.Services.Auditing;

public interface IAuditService
{
    public Task RegisterEventAsync(
        string name,
        EventState state,
        Result? result,
        Guid? userId,
        string? payload = null,
        string? userAgent = null,
        string? ipAddress = null
    );
}