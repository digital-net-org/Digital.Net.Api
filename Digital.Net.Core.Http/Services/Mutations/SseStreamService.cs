using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Digital.Net.Core.Entities.Mutations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Core.Http.Services.Mutations;

public class SseStreamService(ILogger<SseStreamService> logger)
{
    private const string EventType = "mutation";
    private const int ClientQueueCapacity = 256;
    private const int MaxConnectionsPerUser = 10;
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(20);
    private readonly ConcurrentDictionary<Guid, SseClient> _clients = new();

    private readonly ConcurrentDictionary<Guid, int> _connectionsByUser = new();

    public void Broadcast(MutationSignal signal)
    {
        PreparedSignal? prepared = null;
        foreach (var (_, client) in _clients)
        {
            if (!client.Accepts(signal)) continue;
            prepared ??= PreparedSignal.From(signal); // serialize once, shared across all matching clients
            client.Queue.Writer.TryWrite(prepared);
        }
    }

    public async Task StreamAsync(
        HttpContext httpContext,
        IReadOnlySet<string>? entityTypes,
        Func<CancellationToken, Task<IReadOnlyList<MutationSignal>>> catchUp,
        CancellationToken cancellationToken,
        Guid? userId = null
    )
    {
        var response = httpContext.Response;
        if (userId is { } reserveUser && !TryReserveSlot(reserveUser))
        {
            response.StatusCode = StatusCodes.Status429TooManyRequests;
            logger.LogWarning("SSE connection refused for user {UserId}: per-user limit reached", reserveUser);
            return;
        }

        httpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
        response.Headers.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";
        response.Headers["X-Accel-Buffering"] = "no"; // tell proxy not to buffer the stream

        var client = new SseClient(entityTypes, ClientQueueCapacity);
        _clients.TryAdd(client.Id, client);
        logger.LogInformation("SSE client {ClientId} connected ({Count} total)", client.Id, _clients.Count);

        try
        {
            await response.WriteAsync(": connected\n\n", cancellationToken);
            await response.Body.FlushAsync(cancellationToken);

            var caughtUp = new HashSet<Guid>();
            foreach (var signal in await catchUp(cancellationToken))
            {
                await WriteFrameAsync(response, PreparedSignal.From(signal).Frame, cancellationToken);
                caughtUp.Add(signal.Id);
            }

            // Emit a heartbeat comment every HeartbeatInterval. Needed by the proxy and the client watchdog.
            using var heartbeat = new PeriodicTimer(HeartbeatInterval);
            var reader = client.Queue.Reader;
            var readTask = reader.WaitToReadAsync(cancellationToken).AsTask();
            var tickTask = heartbeat.WaitForNextTickAsync(cancellationToken).AsTask();
            while (true)
            {
                var completed = await Task.WhenAny(readTask, tickTask);
                if (completed == tickTask)
                {
                    if (!await tickTask) break;
                    await response.WriteAsync(": ping\n\n", cancellationToken);
                    await response.Body.FlushAsync(cancellationToken);
                    tickTask = heartbeat.WaitForNextTickAsync(cancellationToken).AsTask();
                    continue;
                }

                if (!await readTask) break; // channel completed
                while (reader.TryRead(out var prepared))
                {
                    if (caughtUp.Contains(prepared.Id)) continue;
                    await WriteFrameAsync(response, prepared.Frame, cancellationToken);
                }

                readTask = reader.WaitToReadAsync(cancellationToken).AsTask();
            }
        }
        catch (OperationCanceledException)
        {
            // client disconnected
        }
        catch (IOException)
        {
            // client disconnected (reset mid-write case)
        }
        finally
        {
            _clients.TryRemove(client.Id, out _);
            client.Queue.Writer.TryComplete();
            if (userId is { } releaseUser) ReleaseSlot(releaseUser);
            logger.LogInformation("SSE client {ClientId} disconnected ({Count} total)", client.Id, _clients.Count);
        }
    }

    private bool TryReserveSlot(Guid userId)
    {
        var count = _connectionsByUser.AddOrUpdate(userId, 1, (_, current) => current + 1);
        if (count <= MaxConnectionsPerUser) return true;
        ReleaseSlot(userId); // roll back the over-limit increment
        return false;
    }

    private void ReleaseSlot(Guid userId)
    {
        var remaining = _connectionsByUser.AddOrUpdate(userId, 0, (_, current) => current - 1);
        if (remaining <= 0) _connectionsByUser.TryRemove(new KeyValuePair<Guid, int>(userId, remaining));
    }

    private static async Task WriteFrameAsync(HttpResponse response, string frame, CancellationToken ct)
    {
        await response.WriteAsync(frame, ct);
        await response.Body.FlushAsync(ct);
    }

    private sealed class PreparedSignal
    {
        private PreparedSignal(Guid id, string frame)
        {
            Id = id;
            Frame = frame;
        }

        public Guid Id { get; }

        // One frame for every client: the originating tab is identified by OriginClientId carried in the
        // payload, so the client (not the server) decides whether to drop its own echo.
        public string Frame { get; }

        public static PreparedSignal From(MutationSignal signal)
        {
            var data = JsonSerializer.Serialize(new
            {
                type = signal.ChangeType.ToString(),
                entity = signal.EntityType,
                entityId = signal.EntityId,
                originClientId = signal.OriginClientId
            });
            var frame = $"id: {MutationCursor.From(signal).Format()}\nevent: {EventType}\ndata: {data}\n\n";
            return new PreparedSignal(signal.Id, frame);
        }
    }

    private sealed class SseClient
    {
        private readonly IReadOnlySet<string>? _entityTypes;

        public SseClient(IReadOnlySet<string>? entityTypes, int capacity)
        {
            _entityTypes = entityTypes;
            Queue = Channel.CreateBounded<PreparedSignal>(
                new BoundedChannelOptions(capacity)
                    { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true });
        }

        public Guid Id { get; } = Guid.NewGuid();
        public Channel<PreparedSignal> Queue { get; }
        public bool Accepts(MutationSignal signal) => _entityTypes is null || _entityTypes.Contains(signal.EntityType);
    }
}