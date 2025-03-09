namespace Digital.Core.Api.Test.Api;

public static class RootApi
{
    public const string BaseUrl = "/";

    public static async Task<HttpResponseMessage> GetAppStatus(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}");

    public static async Task<HttpResponseMessage> GetAppVersion(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}version");
}