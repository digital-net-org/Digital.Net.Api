using System.Net.Http;
using System.Threading.Tasks;

namespace Digital.Net.Tests.Core.Sdk;

public static class RootApi
{
    public const string BaseUrl = "/";

    public static async Task<HttpResponseMessage> GetAppVersion(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}");
}