using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.HttpContext.Extensions;

public static class RequestHeadersExtensions
{
    /// <summary>
    ///     Test if "If-None-Match" header is equal to provided etag.
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="etag"></param>
    /// <returns>True if strict equality, false otherwise</returns>
    public static bool TestIfNoneMatch(this IHeaderDictionary headers, string? etag) => 
        headers.TryGetValue("If-None-Match", out var v) && v.ToString() == etag;
}