using Digital.Net.Api.Core.Extensions.HttpUtilities;
using Digital.Net.Tests.Core;

namespace Digital.Net.Api.Core.Test.Extensions.HttpUtilities;

public class ClientHeadersTests : UnitTest
{
    [Test]
    public async Task TryGetHeaderValue_ReturnsValue_WhenHeaderExists()
    {
        var response = new HttpResponseMessage();
        response.Headers.Add("Test-Header", "TestValue");
        var result = response.TryGetHeaderValue("Test-Header");
        await Assert.That(result).IsEqualTo("TestValue");
    }

    [Test]
    public async Task TryGetHeaderValue_ReturnsNull_WhenHeaderDoesNotExist()
    {
        var response = new HttpResponseMessage();
        var result = response.TryGetHeaderValue("Non-Existent-Header");
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryGetCookie_ReturnsCookieValue_WhenSetCookieHeaderExists()
    {
        var response = new HttpResponseMessage();
        response.Headers.Add("Set-Cookie", "cookieValue");
        var result = response.TryGetCookie();
        await Assert.That(result).IsEqualTo("cookieValue");
    }

    [Test]
    public async Task TryGetCookie_ReturnsNull_WhenSetCookieHeaderDoesNotExist()
    {
        var response = new HttpResponseMessage();
        var result = response.TryGetCookie();
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task AddCookie_AddsCookieToClientHeaders()
    {
        const string cookie = "testCookie";
        var client = new HttpClient();
        client.AddCookie(cookie);
        await Assert.That(client.DefaultRequestHeaders.Contains(ClientHeaders.CookieHeader)).IsTrue();
        await Assert.That(client.DefaultRequestHeaders.GetValues(ClientHeaders.CookieHeader).First()).IsEqualTo(cookie);
    }

    [Test]
    public async Task AddAuthorization_AddsBearerTokenToClientHeaders()
    {
        var client = new HttpClient();
        const string token = "testToken";
        client.AddAuthorization(token);
        await Assert.That(client.DefaultRequestHeaders.Authorization).IsNotNull();
        await Assert.That(client.DefaultRequestHeaders.Authorization.Scheme).IsEqualTo(ClientHeaders.BearerAuthorization);
        await Assert.That(client.DefaultRequestHeaders.Authorization.Parameter).IsEqualTo(token);
    }
}