using System.Net;
using Digital.Net.Api.Authentication.Events;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Authentication.Tests.Endpoints;

public class JwtLogoutTest : AuthenticationTest
{
    [Test]
    public async Task Logout_ShouldLogoutClient()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();
        await client.Login(user);

        await ExecuteTestAsync(
            await client.Logout(),
            user,
            AuthenticationEvents.Logout
        );
    }

    [Test]
    public async Task LogoutAll_ShouldLogoutAllClients()
    {
        var client = Application.CreateClient();
        var secondClient = Application.CreateClient();
        var user = Application.CreateUser();
        await client.Login(user);
        await secondClient.Login(user);

        await ExecuteTestAsync(
            await client.LogoutAll(),
            user,
            AuthenticationEvents.LogoutAll
        );
    }

    private async Task ExecuteTestAsync(
        HttpResponseMessage result,
        User user,
        string eventType
    )
    {
        var logoutEvent = GetUserEvents(user).First();
        await Assert.That(result.StatusCode).EqualTo(HttpStatusCode.NoContent);
        await Assert.That(logoutEvent.Name).EqualTo(eventType);
        await Assert.That(logoutEvent.State).EqualTo(EventState.Success);
        await Assert.That(GetUserTokens(user)).IsEmpty();
    }
}