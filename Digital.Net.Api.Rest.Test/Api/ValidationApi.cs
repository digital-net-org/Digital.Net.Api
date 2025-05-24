namespace Digital.Net.Api.Rest.Test.Api;

public static class ValidationApi
{
    public const string BaseUrl = "/validation";

    public static async Task<HttpResponseMessage> GetPasswordPattern(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}/password/pattern");
}