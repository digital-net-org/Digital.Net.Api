using System.Net;
using Digital.Net.Api.Authentication.Events;
using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Authentication.Tests.Endpoints;

public class JwtSessionsTest : AuthenticationTest
{
    [Test]
    public async Task Login_OnMaxCurrentSessions_ShouldInvalidateOldestSession()
    {
        const int maxSessions = AuthenticationStaticOptions.MaxConcurrentSessions;
        var clients = new List<HttpClient>();
        var user = Application.CreateUser();

        for (var i = 0; i < maxSessions + 1; i++)
        {
            var c = Application.CreateClient();
            clients.Add(c);
            await c.Login(user);
        }

        var successCount = await Application
            .GetRepository<Event>()
            .CountAsync(e => e.UserId == user.Id
                             && e.Name == AuthenticationEvents.Login
                             && e.State == EventState.Success
            );

        await Assert.That(successCount).EqualTo(maxSessions + 1);

        var failure = await clients.First().RefreshTokens();
        await Assert.That(failure.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}