using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Core.Entities.Mutations;

public class MutationBroadcaster(ILogger<MutationBroadcaster> logger)
{
    public const string Channel = "mutation";

    public async Task PublishAsync(
        DbContext context,
        IReadOnlyList<MutationSignal> signals,
        CancellationToken cancellationToken = default
    )
    {
        if (signals.Count == 0)
            return;

        try
        {
            var payloads = signals.Select(signal => JsonSerializer.Serialize(signal)).ToArray();
            await context.Database.ExecuteSqlRawAsync(
                "SELECT pg_notify({0}, payload) FROM unnest({1}) AS payload",
                [Channel, payloads],
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish {Count} mutation signal(s)", signals.Count);
        }
    }
}