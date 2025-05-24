using System.Net;
using Digital.Net.Api.Controllers.Controllers.UserApi.Dto;
using Digital.Net.Api.Rest.Test.Api;
using Digital.Net.Api.TestUtilities.Data.Factories;
using Digital.Net.Api.TestUtilities.Integration;

namespace Digital.Net.Api.Rest.Test.Integration.Authentication.JwtTests;

public class JwtAuthorizationTest(AppFactory<Program> fixture) : AuthenticationTest(fixture)
{
    [Fact]
    public async Task LoggedUser_OnProtectedRoute_ShouldBeAuthorized()
    {
        await BaseClient.Login(UserRepository.BuildTestUser());
        var response = await BaseClient.GetUsers(new UserQuery());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
