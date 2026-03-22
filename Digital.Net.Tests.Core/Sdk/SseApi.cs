using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Digital.Net.Tests.Core.Sdk;

public static class SseApi
{
    public const string CmsStreamUrl = "/cms/events/stream";

    public static async Task<HttpResponseMessage> ConnectSseStream(
        this HttpClient client,
        string url = CmsStreamUrl
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }

    public static async Task<HttpResponseMessage> ConnectSseStreamWithLastEventId(
        this HttpClient client,
        long lastEventId,
        string url = CmsStreamUrl
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Last-Event-ID", lastEventId.ToString());
        return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }

    public static async Task<string?> ReadSseEvent(
        this HttpResponseMessage response,
        TimeSpan timeout
    )
    {
        using var cts = new CancellationTokenSource(timeout);
        var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        string? dataLine = null;
        try
        {
            while (await reader.ReadLineAsync(cts.Token) is { } line)
            {
                if (line.StartsWith("data: "))
                    dataLine = line["data: ".Length..];
                if (line == "" && dataLine is not null)
                    return dataLine;
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout — no event received
        }

        return null;
    }
}
