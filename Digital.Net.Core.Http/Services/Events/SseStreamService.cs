using Digital.Net.Core.Services.Events;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Core.Http.Services.Events;

public class SseStreamService : ISseStreamService, IDisposable
{
    private const int EventBufferCapacity = 50;

    private readonly IEventSignalService _signalService;
    private readonly ILogger<SseStreamService> _logger;
    private readonly ConcurrentDictionary<string, SseClient> _clients = new();
    private readonly LinkedList<BufferedEvent> _eventBuffer = new();
    private readonly Lock _bufferLock = new();
    private long _eventId;

    public SseStreamService(IEventSignalService signalService, ILogger<SseStreamService> logger)
    {
        _signalService = signalService;
        _logger = logger;
        _signalService.OnSignal += HandleSignal;
    }

    public Task SubscribeAsync(
        HttpResponse response,
        string eventType,
        Func<EventSignal, bool> filter,
        CancellationToken cancellationToken
    ) => SubscribeCoreAsync(response, eventType, filter, cancellationToken);

    public void Dispose() => _signalService.OnSignal -= HandleSignal;

    private void HandleSignal(EventSignal signal)
    {
        var id = Interlocked.Increment(ref _eventId);
        var data = JsonSerializer.Serialize(new { Event = signal.Name, OccurredAt = signal.OccurredAt });

        BufferEvent(id, signal, data);

        foreach (var (clientId, client) in _clients)
        {
            if (client.Filter(signal))
                WriteEvent(clientId, client.Writer, id, client.EventType, data);
        }
    }

    private async Task SubscribeCoreAsync(
        HttpResponse response,
        string eventType,
        Func<EventSignal, bool> filter,
        CancellationToken cancellationToken
    )
    {
        var clientId = Guid.NewGuid().ToString();

        response.Headers.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";

        var writer = new StreamWriter(response.Body) { AutoFlush = true };

        writer.Write(": connected\n\n");
        ReplayMissedEvents(response.HttpContext.Request, writer, clientId, eventType, filter);
        _clients.TryAdd(clientId, new SseClient(writer, eventType, filter));

        _logger.LogInformation(
            "SSE client {ClientId} connected. Total clients: {Count}",
            clientId,
            _clients.Count
        );

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Client disconnected
        }
        finally
        {
            _clients.TryRemove(clientId, out _);
            _logger.LogInformation(
                "SSE client {ClientId} disconnected. Total clients: {Count}",
                clientId,
                _clients.Count
            );
        }
    }

    private void WriteEvent(string clientId, StreamWriter writer, long id, string eventType, string data)
    {
        try
        {
            writer.Write($"id: {id}\nevent: {eventType}\ndata: {data}\n\n");
        }
        catch
        {
            _clients.TryRemove(clientId, out _);
            _logger.LogWarning(
                "SSE client {ClientId} removed (write failure). Total clients: {Count}",
                clientId,
                _clients.Count
            );
        }
    }

    private void BufferEvent(long id, EventSignal signal, string data)
    {
        lock (_bufferLock)
        {
            _eventBuffer.AddLast(new BufferedEvent(id, signal, data));
            while (_eventBuffer.Count > EventBufferCapacity)
                _eventBuffer.RemoveFirst();
        }
    }

    private void ReplayMissedEvents(
        HttpRequest request,
        StreamWriter writer,
        string clientId,
        string eventType,
        Func<EventSignal, bool> filter
    )
    {
        var lastEventIdHeader = request.Headers["Last-Event-ID"].FirstOrDefault();
        if (!long.TryParse(lastEventIdHeader, out var lastEventId))
            return;

        lock (_bufferLock)
        {
            foreach (var buffered in _eventBuffer.Where(b => b.Id > lastEventId && filter(b.Signal)))
                WriteEvent(clientId, writer, buffered.Id, eventType, buffered.Data);
        }
    }

    private sealed record SseClient(StreamWriter Writer, string EventType, Func<EventSignal, bool> Filter);
    private sealed record BufferedEvent(long Id, EventSignal Signal, string Data);
}
