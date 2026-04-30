using System.Net;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Services.Authentication.Events;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Endpoints.Authentication;

public class LogoutTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }
    
    [Test]
    public async Task Logout_ShouldLogoutClient()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await client.Login(user);

        var result = await client.Logout();
        await ExecuteTestAsync(result, user, AuthenticationEvents.Logout);
    }

    [Test]
    public async Task LogoutAll_ShouldLogoutAllClients()
    {
        var client = ApplicationFixture.CreateClient();
        var secondClient = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await client.Login(user);
        await secondClient.Login(user);

        var result = await client.LogoutAll();
        await ExecuteTestAsync(result, user, AuthenticationEvents.LogoutAll);
    }

    private async Task ExecuteTestAsync(
        HttpResponseMessage result,
        User user,
        string eventType
    )
    {
        var logoutEvent = ApplicationFixture
            .GetContext().Events
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .First();
        var userTokens = ApplicationFixture
            .GetContext().ApiTokens
            .Where(x => x.UserId == user.Id)
            .ToList();
        
        await Assert.That(result.StatusCode).EqualTo(HttpStatusCode.NoContent);
        await Assert.That(logoutEvent.Name).EqualTo(eventType);
        await Assert.That(logoutEvent.State).EqualTo(EventState.Success);
        await Assert.That(userTokens).IsEmpty();
    }
}