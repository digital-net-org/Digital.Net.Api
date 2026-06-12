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
    private readonly ConcurrentDictionary<Guid, SseClient> _clients = new();

    public void Broadcast(MutationSignal signal)
    {
        foreach (var (_, client) in _clients)
            if (client.Accepts(signal))
                client.Queue.Writer.TryWrite(signal); // bounded + DropOldest → best-effort, never blocks the listener
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
        httpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
        response.Headers.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";

        var client = new SseClient(entityTypes, ClientQueueCapacity, userId);
        _clients.TryAdd(client.Id, client);
        logger.LogInformation("SSE client {ClientId} connected ({Count} total)", client.Id, _clients.Count);

        try
        {
            await response.WriteAsync(": connected\n\n", cancellationToken);
            await response.Body.FlushAsync(cancellationToken);

            var caughtUp = new HashSet<Guid>();
            foreach (var signal in await catchUp(cancellationToken))
            {
                await WriteAsync(response, signal, client.UserId, cancellationToken);
                caughtUp.Add(signal.Id);
            }

            await foreach (var signal in client.Queue.Reader.ReadAllAsync(cancellationToken))
            {
                if (caughtUp.Contains(signal.Id)) continue;
                await WriteAsync(response, signal, client.UserId, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // client disconnected — expected
        }
        finally
        {
            _clients.TryRemove(client.Id, out _);
            client.Queue.Writer.TryComplete();
            logger.LogInformation("SSE client {ClientId} disconnected ({Count} total)", client.Id, _clients.Count);
        }
    }

    private static async Task WriteAsync(HttpResponse response, MutationSignal signal, Guid? clientUserId,
        CancellationToken ct)
    {
        var isSelf = signal.UserId is { } actor && actor != Guid.Empty && actor == clientUserId;
        var data = JsonSerializer.Serialize(new
        {
            type = signal.ChangeType.ToString(),
            entity = signal.EntityType,
            entityId = signal.EntityId,
            isSelf
        });
        await response.WriteAsync(
            $"id: {MutationCursor.From(signal).Format()}\nevent: {EventType}\ndata: {data}\n\n", ct
        );
        await response.Body.FlushAsync(ct);
    }

    private sealed class SseClient
    {
        private readonly IReadOnlySet<string>? _entityTypes;

        public SseClient(IReadOnlySet<string>? entityTypes, int capacity, Guid? userId)
        {
            _entityTypes = entityTypes;
            UserId = userId;
            Queue = Channel.CreateBounded<MutationSignal>(
                new BoundedChannelOptions(capacity)
                    { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true });
        }

        public Guid Id { get; } = Guid.NewGuid();
        public Guid? UserId { get; }
        public Channel<MutationSignal> Queue { get; }
        public bool Accepts(MutationSignal signal) => _entityTypes is null || _entityTypes.Contains(signal.EntityType);
    }
}