using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Http;

public static class ClientHeaders
{
    /// <summary>
    ///     Retrieves the User-Agent header value from the HTTP request.
    /// </summary>
    /// <param name="context">The <see cref="IHttpContextAccessor" /> instance to access the HTTP context.</param>
    /// <returns>
    ///     The value of the User-Agent header as a string, or null if the header is not present or empty.
    /// </returns>
    public static string? GetUserAgent(this HttpContext context)
    {
        var result = context.Request.Headers.UserAgent.ToString();
        return string.IsNullOrEmpty(result) ? null : result;
    }

    /// <summary>
    ///     Retrieves the remote IP address of the client making the HTTP request.
    /// </summary>
    /// <param name="context">The <see cref="IHttpContextAccessor" /> instance to access the HTTP context.</param>
    /// <returns>
    ///     property if the header is not present.
    /// </returns>
    public static string? GetRemoteIpAddress(this HttpContext context)
    {
        context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor);
        return
            forwardedFor.FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString();
    }
}