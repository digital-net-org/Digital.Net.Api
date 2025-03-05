namespace Digital.Core.Api.Test.Collections;

public static class RootCollection
{
    public const string BaseUrl = "/";

    public static async Task<HttpResponseMessage> GetAppStatus(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}");

    public static async Task<HttpResponseMessage> GetAppVersion(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}version");
}