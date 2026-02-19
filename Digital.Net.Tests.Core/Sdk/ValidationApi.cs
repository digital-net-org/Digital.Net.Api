using System.Net.Http;
using System.Threading.Tasks;

namespace Digital.Net.Tests.Core.Sdk;

public static class ValidationApi
{
    public const string BaseUrl = "/validation";

    public static async Task<HttpResponseMessage> GetPasswordPattern(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}/password/pattern");
}