using System.Net;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Authentication.Tests.Endpoints.JwtTests;

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
}