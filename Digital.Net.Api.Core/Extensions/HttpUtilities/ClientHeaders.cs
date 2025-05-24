using System.Net.Http.Headers;
using Digital.Net.Api.Core.Errors;

namespace Digital.Net.Api.Core.Extensions.HttpUtilities;

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
    public static string? TryGetHeaderValue(this HttpResponseMessage responseMessage, string key) =>
        TryCatchUtilities.TryOrNull(() => responseMessage.Headers.GetValues(key).FirstOrDefault());

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
}