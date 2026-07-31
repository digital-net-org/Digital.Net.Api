using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Digital.Net.Tests.Core.Http;

public static class HttpContentExtensions
{
    public const string SetCookieHeader = "Set-Cookie";
    public const string CookieHeader = "Cookie";

    public static async Task<T> ReadContentAsync<T>(this HttpContent content)
    {
        var value = await content.ReadAsStringAsync();
        return HttpContentSerializer.Deserialize<T>(value);
    }

    /// <summary>Every Set-Cookie directive of the response, attributes included.</summary>
    public static IEnumerable<string> GetSetCookies(this HttpResponseMessage response) =>
        response.Headers.TryGetValues(SetCookieHeader, out var values) ? values : [];

    /// <summary>The named cookie's raw directive, or null when the response does not set it.</summary>
    public static string? TryGetSetCookie(this HttpResponseMessage response, string name) =>
        response.GetSetCookies().FirstOrDefault(c => c.StartsWith($"{name}=", StringComparison.Ordinal));

    /// <summary>The named cookie's value alone. Empty on a deletion directive.</summary>
    public static string? TryGetCookieValue(this HttpResponseMessage response, string name) =>
        response.TryGetSetCookie(name)?.Split(';')[0][(name.Length + 1)..];

    public static void SetCookie(this HttpClient client, string name, string value)
    {
        client.DefaultRequestHeaders.Remove(CookieHeader);
        client.DefaultRequestHeaders.Add(CookieHeader, $"{name}={value}");
    }

    public static void ClearCookies(this HttpClient client) =>
        client.DefaultRequestHeaders.Remove(CookieHeader);
}
