using Digital.Net.Core.Entities.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Digital.Net.Core.Http.Bootstrap;

public sealed class DatabaseHealthCheck(DigitalContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext _,
        CancellationToken cancellationToken = default
    ) =>
        await context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Database unreachable");
}