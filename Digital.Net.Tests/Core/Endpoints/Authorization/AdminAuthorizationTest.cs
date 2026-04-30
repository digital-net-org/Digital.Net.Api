using System.Net;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Endpoints.Authorization;

public class AdminAuthorizationTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task Authorize_WithValidApiKey_ShouldReturnOk()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });

        await client.Login(user);
        var response = await client.TestAdminAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }
}