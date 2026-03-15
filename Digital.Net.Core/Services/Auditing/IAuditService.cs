using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Core.Services.Auditing;

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