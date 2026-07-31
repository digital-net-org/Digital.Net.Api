using System.Net;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Http.Endpoints.Authorization;

public class AdminAuthorizationTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task AdminSession_OnAdminRoute_ShouldBeAuthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });

        await client.Login(user);
        var response = await client.TestAdminAuthorization();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task NonAdminSession_OnAdminRoute_ShouldBeForbidden()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = false });

        await client.Login(user);
        var response = await client.TestAdminAuthorization();

        // 403, not 401: the caller is authenticated, just not allowed.
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task NonAdminApiKey_OnAdminRoute_ShouldBeForbidden()
    {
        // The admin check must not depend on which scheme authenticated the caller.
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = false });
        var apiKey = Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128);
        var context = ApplicationFixture.GetContext();
        await context.ApiKeys.AddAsync(new ApiKey(user.Id, "admin-check-key", apiKey));
        await context.SaveChangesAsync();

        var client = ApplicationFixture.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApiKeyHeaderAccessor, apiKey);

        var response = await client.TestAdminAuthorization();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Anonymous_OnAdminRoute_ShouldBeUnauthorized()
    {
        var response = await ApplicationFixture.CreateClient().TestAdminAuthorization();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}
