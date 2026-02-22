using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Core.Http;

public static class ClientHeaders
{
    public const string SetCookieHeader = "Set-Cookie";
    public const string CookieHeader = "Cookie";
    public const string BearerAuthorization = "Bearer";

    /// <summary>
    ///     Try to get a header value from the header's collection.
    /// </summary>
    /// <param name="responseMessage">The response message to get the header value from.</param>
    /// <param name="key">The key of the header to get the value for.</param>
    /// <returns>The value of the header if it exists, otherwise null.</returns>
    public static string? TryGetHeaderValue(this HttpResponseMessage responseMessage, string key)
    {
        try
        {
            return responseMessage.Headers.GetValues(key).FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Try to get a cookie from the response headers.
    /// </summary>
    /// <param name="headers">The response message to get the cookie from.</param>
    /// <returns>The cookie if it exists, otherwise null.</returns>
    public static string? TryGetCookie(this HttpResponseMessage headers) =>
        headers.TryGetHeaderValue(SetCookieHeader);

    /// <summary>
    ///     Add a cookie to the Client headers.
    /// </summary>
    /// <param name="client">The HttpClient to add the cookie to.</param>
    /// <param name="cookie">The cookie to add to the request headers.</param>
    public static void AddCookie(this HttpClient client, string cookie) =>
        client.DefaultRequestHeaders.Add(CookieHeader, cookie);

    /// <summary>
    ///     Add a Bearer token to the Client headers.
    /// </summary>
    /// <param name="client">The HttpClient to add the token to.</param>
    /// <param name="token">The token to add to the request headers.</param>
    public static void AddAuthorization(this HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BearerAuthorization, token);
    
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