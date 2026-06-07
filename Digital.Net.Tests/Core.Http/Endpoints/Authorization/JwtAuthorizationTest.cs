using System.Net;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Tests.Core.Http.Endpoints.Authorization;

public class JwtAuthorizationTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task LoggedUser_OnProtectedRoute_ShouldBeAuthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();

        await client.Login(user);
        var response = await client.TestJwtAuthorization();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task InactiveUser_OnLoginRoute_ShouldNotBeAuthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = false });
        var response = await client.Login(user);
        var loginEvent = await ApplicationFixture
            .GetContext().AuthEvents
            .Where(x => x.UserId == user.Id).OrderByDescending(x => x.CreatedAt)
            .FirstAsync();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(loginEvent.Type).EqualTo(AuthEventType.Login);
        await Assert.That(loginEvent.Success).IsFalse();
    }

    [Test]
    public async Task InactiveUser_OnProtectedRoute_ShouldNotBeAuthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true });
        await client.Login(user);

        var context = ApplicationFixture.GetContext();
        var userInDb = await context.Users.FindAsync(user.Id);
        userInDb!.IsActive = false;
        context.Users.Update(userInDb);
        await context.SaveChangesAsync();

        var response = await client.TestJwtAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}
