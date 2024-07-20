using Microsoft.AspNetCore.Http;

namespace SafariDigital.Core.Http;

public static class HttpContextUtils
{
    public static string? GetRemoteIpAddressFromRequest(HttpRequest request)
    {
        request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor);
        var ipAddress = forwardedFor.FirstOrDefault() ??
                        request.HttpContext.Connection.RemoteIpAddress?.ToString();
        return ipAddress;
    }

    public static string? GetRemoteIpAddress(this HttpContext context) =>
        GetRemoteIpAddressFromRequest(context.Request);

    public static string? GetRemoteIpAddress(this HttpRequest request) =>
        GetRemoteIpAddressFromRequest(request);

    public static string? GetUserAgentFromRequest(HttpRequest request) =>
        request.Headers.UserAgent.ToString();

    public static string? GetUserAgent(this HttpContext context) =>
        GetUserAgentFromRequest(context.Request);

    public static string? GetUserAgent(this HttpRequest request) =>
        GetUserAgentFromRequest(request);
}