using System.Net;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Authentication.Test.Endpoints.AdminTests;

public class AdminAuthorizationTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    [Test]
    public async Task Authorize_WithValidApiKey_ShouldReturnOk()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });

        await client.Login(user);
        var response = await client.TestAdminAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }
}