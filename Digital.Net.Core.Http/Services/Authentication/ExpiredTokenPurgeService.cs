using Digital.Net.Core.Entities.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Core.Http.Services.Authentication;

public class ExpiredTokenPurgeService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredTokenPurgeService> logger
) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);
            await PurgeExpiredTokensAsync();
        }
    }

    private async Task PurgeExpiredTokensAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DigitalContext>();
        var expired = context.ApiTokens.Where(t => t.ExpiredAt < DateTime.UtcNow).ToList();

        if (expired.Count == 0)
            return;

        context.ApiTokens.RemoveRange(expired);
        await context.SaveChangesAsync();
        logger.LogInformation("Purged {Count} expired refresh token(s)", expired.Count);
    }
}
