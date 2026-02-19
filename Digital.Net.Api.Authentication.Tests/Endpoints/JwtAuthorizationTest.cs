using System.Net;
using Digital.Net.Api.Controllers.Controllers.UserApi.Dto;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Authentication.Tests.Endpoints;

public class JwtAuthorizationTest : AuthenticationTest
{
    [Test]
    public async Task LoggedUser_OnProtectedRoute_ShouldBeAuthorized()
    {
        var client = Application.CreateClient();
        await client.Login(Application.CreateUser());
        var response = await client.GetUsers(new UserQuery());
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }
}