namespace Digital.Net.Api.Rest.Test.Api;

public static class RootApi
{
    public const string BaseUrl = "/";

    public static async Task<HttpResponseMessage> GetAppVersion(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}");
}