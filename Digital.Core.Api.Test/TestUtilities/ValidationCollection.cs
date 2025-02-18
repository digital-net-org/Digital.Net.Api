namespace Digital.Core.Api.Test.TestUtilities;

public static class ValidationCollection
{
    public const string BaseUrl = "/validation";

    public static async Task<HttpResponseMessage> GetPasswordPattern(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}/password/pattern");
}