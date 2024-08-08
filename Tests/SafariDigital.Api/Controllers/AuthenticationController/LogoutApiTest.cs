using System.Net;
using Safari.Net.TestTools.Integration;
using SafariDigital.Api;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models.Database;
using Tests.Utils.ApiCollections;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Api.Controllers.AuthenticationController;

public class LogoutApiTest : IntegrationTest<Program, SafariDigitalContext>
{
    private readonly UserFactory _userFactory;

    public LogoutApiTest(AppFactory<Program, SafariDigitalContext> fixture) : base(fixture)
    {
        SafariDigitalRepository<User> userRepository = new(GetContext());
        _userFactory = new UserFactory(userRepository);
    }

    [Fact]
    public async Task Logout_ShouldLogoutClient()
    {
        var (user, password) = _userFactory.CreateUser();
        await BaseClient.Login(user.Username, password);

        var logoutResponse = await BaseClient.Logout();
        var cookies = BaseClient.DefaultRequestHeaders.GetValues("Cookie");

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        Assert.DoesNotContain("RefreshToken", cookies);
    }
}