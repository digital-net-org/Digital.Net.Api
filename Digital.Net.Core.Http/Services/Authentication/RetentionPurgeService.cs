using Digital.Net.Core.Entities.Context;
using Digital.Net.Lib.Configuration;
using Digital.Net.Lib.Entities.Mutations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Digital.Net.Core.Http.Services.Authentication;

public class RetentionPurgeService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<RetentionPurgeService> logger
) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                await Task.Delay(Interval, stoppingToken);
                await PurgeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Retention purge failed; retrying next tick");
            }
    }

    private async Task PurgeAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DigitalContext>();

        var now = DateTime.UtcNow;
        var sessions = await context.Sessions.Where(s => s.ExpiredAt < now).ExecuteDeleteAsync(ct);

        var retentionDays = configuration.Get<int?>(CoreSettings.AuditRetentionDaysKey)
                            ?? CoreSettings.DefaultAuditRetentionDays;
        var cutoff = now.AddDays(-retentionDays);

        var mutations = 0;
        foreach (var schema in scope.ServiceProvider.GetServices<MutationSchema>().Select(s => s.Name).Distinct())
            mutations += await context.Database.ExecuteSqlRawAsync(
                $"DELETE FROM {schema}.\"EntityMutation\" WHERE \"CreatedAt\" < @cutoff",
                [new NpgsqlParameter("cutoff", NpgsqlDbType.TimestampTz) { Value = cutoff }],
                ct);

        var authEvents = await context.AuthEvents.Where(e => e.CreatedAt < cutoff).ExecuteDeleteAsync(ct);
        if (sessions + mutations + authEvents > 0)
            logger.LogInformation(
                "Retention purge: {Sessions} session(s), {Mutations} mutation(s), {AuthEvents} auth event(s)",
                sessions, mutations, authEvents);
    }
}