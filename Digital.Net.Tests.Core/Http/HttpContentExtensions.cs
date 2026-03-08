using System.Net.Http;
using System.Threading.Tasks;

namespace Digital.Net.Tests.Core.Http;

public static class HttpContentExtensions
{
    public static async Task<T> ReadContentAsync<T>(this HttpContent content)
    {
        var value = await content.ReadAsStringAsync();
        return HttpContentSerializer.Deserialize<T>(value);
    }
}