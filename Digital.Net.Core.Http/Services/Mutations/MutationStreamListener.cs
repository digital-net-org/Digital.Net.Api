using System.Text.Json;
using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Lib.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Digital.Net.Core.Http.Services.Mutations;

public class MutationStreamListener(
    SseStreamService sseStream,
    IConfiguration configuration,
    ILogger<MutationStreamListener> logger
) : BackgroundService
{
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionString = configuration.GetOrThrow<string>(CoreSettings.ConnectionStringKey);
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync(stoppingToken);
                connection.Notification += OnNotification;

                await using (var listen = new NpgsqlCommand($"LISTEN {MutationBroadcaster.Channel}", connection))
                {
                    await listen.ExecuteNonQueryAsync(stoppingToken);
                }

                logger.LogInformation("Mutation listener subscribed to '{Channel}'", MutationBroadcaster.Channel);
                while (!stoppingToken.IsCancellationRequested)
                    await connection.WaitAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Mutation listener connection lost; reconnecting in {Delay}s",
                    ReconnectDelay.TotalSeconds);
                try
                {
                    await Task.Delay(ReconnectDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
    }

    private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
    {
        try
        {
            if (JsonSerializer.Deserialize<MutationSignal>(e.Payload) is { } signal)
                sseStream.Broadcast(signal);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to handle mutation notification");
        }
    }
}