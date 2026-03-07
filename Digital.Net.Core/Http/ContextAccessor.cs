using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Http;

public static class ContextAccessor
{
    /// <summary>Get the current HttpRequest.</summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static HttpRequest GetRequest(this IHttpContextAccessor contextAccessor) =>
        contextAccessor.GetHttpContext().Request ??
        throw new InvalidOperationException("Http Request is not defined");

    /// <summary>Get the current HttpResponse.</summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static HttpResponse GetResponse(this IHttpContextAccessor contextAccessor) =>
        contextAccessor.GetHttpContext().Response ??
        throw new InvalidOperationException("Http Response is not defined");

    /// <summary>Get the current HttpContext.</summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static HttpContext GetHttpContext(this IHttpContextAccessor contextAccessor) =>
        contextAccessor.HttpContext ??
        throw new InvalidOperationException("Http Context is not defined");
}