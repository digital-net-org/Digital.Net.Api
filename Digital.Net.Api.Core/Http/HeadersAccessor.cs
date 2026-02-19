using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Core.Http;

public static class HeadersAccessor
{
    /// <summary>
    ///     Retrieves the User-Agent header value from the HTTP request.
    /// </summary>
    /// <param name="context">The <see cref="IHttpContextAccessor" /> instance to access the HTTP context.</param>
    /// <returns>
    ///     The value of the User-Agent header as a string, or null if the header is not present or empty.
    /// </returns>
    public static string? GetUserAgent(this IHttpContextAccessor context)
    {
        var request = context.GetRequest();
        var result = request.Headers.UserAgent.ToString();
        return string.IsNullOrEmpty(result) ? null : result;
    }

    /// <summary>
    ///     Retrieves the remote IP address of the client making the HTTP request.
    /// </summary>
    /// <param name="context">The <see cref="IHttpContextAccessor" /> instance to access the HTTP context.</param>
    /// <returns>
    ///     The client IP address as a string, or null if it cannot be determined.
    ///     This method first checks the X-Forwarded-For header for the client IP and falls back to the RemoteIpAddress
    ///     property if the header is not present.
    /// </returns>
    public static string? GetRemoteIpAddress(this IHttpContextAccessor context)
    {
        var request = context.GetRequest();
        request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor);
        return
            forwardedFor.FirstOrDefault()
            ?? request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}