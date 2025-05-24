using Digital.Net.Api.Core.Formatters;

namespace Digital.Net.Api.Services.HttpContext.Extensions;

public static class HttpContentExtensions
{
    public static async Task<T> ReadContentAsync<T>(this HttpContent content)
    {
        var value = await content.ReadAsStringAsync();
        return DigitalSerializer.Deserialize<T>(value);
    }
}