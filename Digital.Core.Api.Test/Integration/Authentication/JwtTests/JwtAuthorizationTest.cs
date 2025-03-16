using System.Net;
using Digital.Core.Api.Controllers.UserApi.Dto;
using Digital.Core.Api.Test.Api;
using Digital.Core.Api.Test.Utils;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Authentication.JwtTests;

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
