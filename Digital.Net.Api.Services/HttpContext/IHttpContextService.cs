using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.HttpContext;

public interface IHttpContextService
{
    /// <summary>
    ///     Add an item to the HttpContext.Items collection.
    ///     The item is serialized to a string before being added.
    /// </summary>
    /// <param name="key">The key to add the item to.</param>
    /// <param name="content">The content to add to the HttpContext.Items collection.</param>
    /// <typeparam name="T">The type of the content to add to the HttpContext.Items collection.</typeparam>
    void AddItem<T>(string key, T content);

    /// <summary>
    ///     Get an item from the HttpContext.Items collection.
    /// </summary>
    /// <param name="key">The key to get the item from.</param>
    /// <typeparam name="T">The type of the item to get from the HttpContext.Items collection.</typeparam>
    /// <returns>The item from the HttpContext.Items collection.</returns>
    T? GetItem<T>(string key);

    /// <summary>
    ///     Get a header value from the current HTTP request.
    /// </summary>
    /// <param name="header">The header to get the value from.</param>
    /// <returns>The value of the header.</returns>
    string? GetHeaderValue(string header);

    /// <summary>
    ///     Set a cookie on the HttpResponse.
    /// </summary>
    /// <param name="content">The content of the cookie.</param>
    /// <param name="name">The name of the cookie.</param>
    /// <param name="expiration">The expiration of the cookie.</param>
    /// <param name="sameSite">The SameSiteMode of the cookie.</param>
    /// <param name="httpOnly">The HttpOnly of the cookie.</param>
    /// <param name="secure">The Secure of the cookie.</param>
    /// <exception cref="ArgumentNullException"></exception>
    void SetResponseCookie(
        string content,
        string name,
        DateTime expiration,
        SameSiteMode? sameSite = null,
        bool? httpOnly = null,
        bool? secure = null
    );

    /// <summary>
    ///     Gets the User-Agent header value from the current HTTP request.
    /// </summary>
    string UserAgent { get; }

    /// <summary>
    ///     Gets the remote IP address of the client making the current HTTP request.
    /// </summary>
    string IpAddress { get; }

    /// <summary>
    ///     Gets the Bearer token from the Authorization header of the current HTTP request.
    /// </summary>
    string? BearerToken { get; }

    /// <summary>
    ///     Gets the current HTTP request.
    /// </summary>
    HttpRequest Request { get; }

    /// <summary>
    ///     Gets the current HTTP response.
    /// </summary>
    HttpResponse Response { get; }

    /// <summary>
    ///     Gets the current HTTP context.
    /// </summary>
    /// <returns>The current HttpContext.</returns>
    Microsoft.AspNetCore.Http.HttpContext GetHttpContext();
}