using System.Net;
using Digital.Net.Api.Authentication.Events;
using Digital.Net.Api.Entities.Models.ApiTokens;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Authentication.Tests.Endpoints.JwtTests;

public class JwtLogoutTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }
    
    [Test]
    public async Task Logout_ShouldLogoutClient()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();
        await client.Login(user);

        var result = await client.Logout();
        await ExecuteTestAsync(result, user, AuthenticationEvents.Logout);
    }

    [Test]
    public async Task LogoutAll_ShouldLogoutAllClients()
    {
        var client = Application.CreateClient();
        var secondClient = Application.CreateClient();
        var user = Application.CreateUser();
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
        var logoutEvent = Application
            .GetRepository<Event>()
            .Get(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .First();
        var userTokens = Application
            .GetRepository<ApiToken>()
            .Get(x => x.UserId == user.Id);
        
        await Assert.That(result.StatusCode).EqualTo(HttpStatusCode.NoContent);
        await Assert.That(logoutEvent.Name).EqualTo(eventType);
        await Assert.That(logoutEvent.State).EqualTo(EventState.Success);
        await Assert.That(userTokens).IsEmpty();
    }
}