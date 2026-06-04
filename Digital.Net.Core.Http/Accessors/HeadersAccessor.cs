using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Http.Accessors;

public static class ClientHeaders
{
    public static string? GetUserAgent(this HttpContext context)
    {
        var result = context.Request.Headers.UserAgent.ToString();
        return string.IsNullOrEmpty(result) ? null : result;
    }

    public static string? GetRemoteIpAddress(this HttpContext context)
    {
        context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor);
        return
            forwardedFor.FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString();
    }
}