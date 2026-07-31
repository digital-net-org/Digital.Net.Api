using System.Net;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Http.Endpoints.Authorization;

public class SseCookieAuthorizationTest
{
    private const string StreamUrl = "events/mutation/stream";

    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task Stream_WithValidSessionCookie_AndNoCsrfHeader_ShouldBeAuthorized()
    {
        var client = ApplicationFixture.CreateClientWithoutCsrfHeader();
        var user = ApplicationFixture.CreateUser();
        await ApplicationFixture.AsLoggedAsync(client, user);

        var response = await GetStreamHeadersAsync(client);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Stream_WithoutCookie_ShouldNotBeAuthorized()
    {
        var response = await GetStreamHeadersAsync(ApplicationFixture.CreateClient());
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Stream_WithUnknownCookie_ShouldNotBeAuthorized()
    {
        var client = ApplicationFixture.CreateClient();
        client.SetCookie(AuthenticationApi.CookieName, "not-a-known-session-id");

        var response = await GetStreamHeadersAsync(client);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    // The stream never completes: read the headers only, then abort.
    private static async Task<HttpResponseMessage> GetStreamHeadersAsync(HttpClient client)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var response = await client.GetAsync(StreamUrl, HttpCompletionOption.ResponseHeadersRead, cts.Token);
        await cts.CancelAsync();
        return response;
    }
}
