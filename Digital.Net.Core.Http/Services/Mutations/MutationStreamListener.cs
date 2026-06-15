using System.Text.Json;
using System.Threading.Channels;
using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Lib.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Digital.Net.Core.Http.Services.Mutations;

public class MutationStreamListener(
    MutationSignalDispatcher dispatcher,
    IConfiguration configuration,
    ILogger<MutationStreamListener> logger
) : BackgroundService
{
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ProbeInterval = TimeSpan.FromSeconds(30);

    // OnNotification (sync Npgsql callback) must never block on handler work — it only enqueues here.
    private readonly Channel<MutationSignal> _dispatchQueue = Channel.CreateBounded<MutationSignal>(
        new BoundedChannelOptions(1024) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true });

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var dispatchLoop = DispatchLoopAsync(stoppingToken);
        try
        {
            var connectionString = BuildListenerConnectionString(
                configuration.GetOrThrow<string>(CoreSettings.ConnectionStringKey));
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
                        if (!await connection.WaitAsync(ProbeInterval, stoppingToken))
                            await ProbeAsync(connection, stoppingToken);
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
        finally
        {
            _dispatchQueue.Writer.TryComplete();
            await dispatchLoop;
        }
    }

    private async Task DispatchLoopAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var signal in _dispatchQueue.Reader.ReadAllAsync(stoppingToken))
                await dispatcher.DispatchAsync(signal, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static string BuildListenerConnectionString(string connectionString) =>
        new NpgsqlConnectionStringBuilder(connectionString)
        {
            KeepAlive = 30,
            TcpKeepAlive = true
        }.ConnectionString;

    private async Task ProbeAsync(NpgsqlConnection connection, CancellationToken ct)
    {
        await using var probe = new NpgsqlCommand("SELECT 1", connection);
        await probe.ExecuteScalarAsync(ct);
        logger.LogDebug("Mutation listener alive on '{Channel}'", MutationBroadcaster.Channel);
    }

    private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
    {
        try
        {
            if (JsonSerializer.Deserialize<MutationSignal>(e.Payload) is { } signal)
                _dispatchQueue.Writer.TryWrite(signal);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse mutation notification");
        }
    }
}