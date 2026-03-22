using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Services.Events;

public interface ISseStreamService
{
    Task SubscribeAsync(
        HttpResponse response,
        string eventType,
        Func<EventSignal, bool> filter,
        CancellationToken cancellationToken
    );
}
