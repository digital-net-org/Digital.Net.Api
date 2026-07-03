using System.Net;
using Digital.Net.Core;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Http.Security;

public class ClientIpResolutionTest
{
    private const string ForwardedForHeader = "X-Forwarded-For";

    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task RemoteIp_OnUntrustedConnection_ShouldIgnoreForwardedForHeader()
    {
        var client = CreateClientWithConnectionIp("203.0.113.10");
        client.DefaultRequestHeaders.Add(ForwardedForHeader, "198.51.100.99");
        var user = ApplicationFixture.CreateUser();

        await client.Login(user.Login, "wrongPassword");

        await Assert.That(GetLastLoginEvent(user).IpAddress).EqualTo("203.0.113.10");
    }

    [Test]
    public async Task Login_OnMaxAttemptsWithVariedForwardedFor_ShouldReturnTooManyRequests()
    {
        var client = CreateClientWithConnectionIp("203.0.113.20");
        var user = ApplicationFixture.CreateUser();

        for (var i = 0; i < AuthenticationStaticOptions.MaxLoginAttempts; i++)
        {
            SetForwardedFor(client, $"198.51.100.{i + 1}");
            await client.Login(user.Login, "wrongPassword");
        }

        SetForwardedFor(client, "198.51.100.200");
        var response = await client.Login(user.Login, "wrongPassword");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task RemoteIp_OnTrustedProxyConnection_ShouldResolveLastForwardedHop()
    {
        await using var factory = new ApplicationFactory(
            ApplicationFixture.Fixture.ConnectionString,
            new Dictionary<string, string?>
            {
                { $"{CoreSettings.ForwardedHeadersKnownProxiesKey}:0", "203.0.113.30" }
            }
        );
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestRemoteIpStartupFilter.Header, "203.0.113.30");
        client.DefaultRequestHeaders.Add(ForwardedForHeader, "198.51.100.41, 198.51.100.42");
        var user = ApplicationFixture.CreateUser();

        await client.Login(user.Login, "wrongPassword");

        // ForwardLimit = 1: only the hop appended by the trusted proxy is consumed, the
        // client-supplied part of the header is never trusted.
        await Assert.That(GetLastLoginEvent(user).IpAddress).EqualTo("198.51.100.42");
    }

    private HttpClient CreateClientWithConnectionIp(string ipAddress)
    {
        var client = ApplicationFixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestRemoteIpStartupFilter.Header, ipAddress);
        return client;
    }

    private static void SetForwardedFor(HttpClient client, string value)
    {
        client.DefaultRequestHeaders.Remove(ForwardedForHeader);
        client.DefaultRequestHeaders.Add(ForwardedForHeader, value);
    }

    private AuthEvent GetLastLoginEvent(User user) =>
        ApplicationFixture
            .GetContext().AuthEvents
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .First();
}