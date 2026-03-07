using Digital.Net.Core.Formatters;

namespace Digital.Net.Core.Services.HttpContext.Extensions;

public static class HttpContentExtensions
{
    public static async Task<T> ReadContentAsync<T>(this HttpContent content)
    {
        var value = await content.ReadAsStringAsync();
        return DigitalSerializer.Deserialize<T>(value);
    }
}