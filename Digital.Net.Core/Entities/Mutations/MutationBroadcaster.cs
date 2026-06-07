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
        foreach (var signal in signals)
            try
            {
                var payload = JsonSerializer.Serialize(signal);
                await context.Database.ExecuteSqlRawAsync(
                    "SELECT pg_notify({0}, {1})",
                    [Channel, payload],
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to publish mutation signal {ChangeType} {EntityType} {EntityId}",
                    signal.ChangeType, signal.EntityType, signal.EntityId
                );
            }
    }
}