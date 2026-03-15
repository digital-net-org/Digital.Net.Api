using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Digital.Net.Tests.Core.Http;

public static class HttpContentExtensions
{
    public const string SetCookieHeader = "Set-Cookie";
    public const string CookieHeader = "Cookie";
    public const string BearerAuthorization = "Bearer";
    
    public static async Task<T> ReadContentAsync<T>(this HttpContent content)
    {
        var value = await content.ReadAsStringAsync();
        return HttpContentSerializer.Deserialize<T>(value);
    }

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

    public static string? TryGetCookie(this HttpResponseMessage response) =>
        response.TryGetHeaderValue(SetCookieHeader);

    public static void AddCookie(this HttpClient client, string cookie) =>
        client.DefaultRequestHeaders.Add(CookieHeader, cookie);

    public static void AddAuthorization(this HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BearerAuthorization, token);
}