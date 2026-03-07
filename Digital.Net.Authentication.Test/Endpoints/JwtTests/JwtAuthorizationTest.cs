using System.Net;
using Digital.Net.Entities.Models.Events;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Authentication.Test.Endpoints.JwtTests;

public class JwtAuthorizationTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }
    
    [Test]
    public async Task LoggedUser_OnProtectedRoute_ShouldBeAuthorized()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();

        await client.Login(user);
        var response = await client.TestJwtAuthorization();
        
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task InactiveUser_OnLoginRoute_ShouldNotBeAuthorized()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser(new TestUserPayload { IsActive = false });
        var response = await client.Login(user);
        var loginEvent = await Application
            .GetRepository<Event>()
            .Get()
            .OrderByDescending(x => x.CreatedAt)
            .FirstAsync();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(loginEvent.State).EqualTo(EventState.Failed);
        await Assert.That(loginEvent.ErrorTrace!.Contains("0x80131500")).IsTrue();
    }

    [Test]
    public async Task InactiveUser_OnProtectedRoute_ShouldNotBeAuthorized()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser(new TestUserPayload { IsActive = true });
        await client.Login(user);

        var userInDb = await Application.GetRepository<User>().GetByIdAsync(user.Id);
        userInDb!.IsActive = false;
        await Application.GetRepository<User>().UpdateAndSaveAsync(userInDb);

        var response = await client.TestJwtAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}