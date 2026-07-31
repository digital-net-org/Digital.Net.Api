using System.Net;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Http.Endpoints;

public class ValidationEndpointsTest
{
    private const string EmailPattern = "/validation/pattern/email";

    private static readonly string[] ProtectedRoutes =
    [
        "/validation/pattern/username",
        "/validation/pattern/password",
        "/validation/pattern/api-key-name",
        "/validation/size/avatar"
    ];

    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task EmailPattern_ShouldBePublic()
    {
        // The only one reachable anonymously: the login screen needs it before any session exists.
        var response = await ApplicationFixture.CreateClient().GetAsync(EmailPattern);
        var result = await response.Content.ReadContentAsync<Result<string>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result.Value).IsNotNullOrEmpty();
    }

    [Test]
    [MethodDataSource(nameof(GetProtectedRoutes))]
    public async Task ProtectedRoute_WithoutSession_ShouldReturnUnauthorized(string route)
    {
        var response = await ApplicationFixture.CreateClient().GetAsync(route);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    [MethodDataSource(nameof(GetProtectedRoutes))]
    public async Task ProtectedRoute_WithSession_ShouldReturnTheRule(string route)
    {
        var client = ApplicationFixture.CreateClient();
        await ApplicationFixture.AsLoggedAsync(client, ApplicationFixture.CreateUser());

        var response = await client.GetAsync(route);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    public static IEnumerable<Func<string>> GetProtectedRoutes() =>
        ProtectedRoutes.Select<string, Func<string>>(route => () => route);
}
