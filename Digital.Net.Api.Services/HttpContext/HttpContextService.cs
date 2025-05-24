using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.HttpContext;

public class HttpContextService(IHttpContextAccessor contextAccessor) : IHttpContextService
{
    public string UserAgent => GetUserAgent() ?? string.Empty;
    public string IpAddress => GetRemoteIpAddress() ?? string.Empty;
    public string? BearerToken => Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

    public void AddItem<T>(string key, T content) =>
        GetHttpContext().Items[key] = JsonSerializer.Serialize(content);

    public T? GetItem<T>(string key) =>
        GetHttpContext().Items[key] is not string item ? default : JsonSerializer.Deserialize<T>(item);

    public HttpRequest Request =>
        GetHttpContext().Request ?? throw new NullReferenceException("Http Request is not defined");

    public HttpResponse Response =>
        GetHttpContext().Response ?? throw new NullReferenceException("Http Response is not defined");

    public Microsoft.AspNetCore.Http.HttpContext GetHttpContext() =>
        contextAccessor.HttpContext ?? throw new NullReferenceException("Http Context is not defined");

    public string? GetHeaderValue(string header) => Request.Headers[header].FirstOrDefault();

    public void SetResponseCookie(
        string content,
        string name,
        DateTime expiration,
        SameSiteMode? sameSite = null,
        bool? httpOnly = null,
        bool? secure = null
    )
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = httpOnly ?? true,
            Secure = secure ?? true,
            SameSite = sameSite ?? SameSiteMode.None,
            Expires = expiration
        };
        Response.Cookies.Append(name, content, cookieOptions);
    }

    private string? GetUserAgent()
    {
        var result = Request.Headers.UserAgent.ToString();
        return string.IsNullOrEmpty(result) ? null : result;
    }

    private string? GetRemoteIpAddress()
    {
        Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor);
        return
            forwardedFor.FirstOrDefault()
            ?? Request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}