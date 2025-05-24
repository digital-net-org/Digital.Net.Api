using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Events;

namespace Digital.Net.Api.Services.Events;

public interface IEventService
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