using System.Net;
using Safari.Net.TestTools.Integration;
using SafariDigital.Api;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models.Database;
using Tests.Utils.ApiCollections;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Api.Controllers.AuthenticationController;

public class RefreshApiTest : IntegrationTest<Program, SafariDigitalContext>
{
    private readonly UserFactory _userFactory;

    public RefreshApiTest(AppFactory<Program, SafariDigitalContext> fixture) : base(fixture)
    {
        SafariDigitalRepository<User> userRepository = new(GetContext());
        _userFactory = new UserFactory(userRepository);
    }

    [Fact]
    public async Task RefreshTokens_WithValidRefreshToken_ShouldReturnToken()
    {
        var (user, password) = _userFactory.CreateUser();
        await BaseClient.Login(user.Username, password);
        var response = await BaseClient.RefreshTokens();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("token", content);
    }

    [Fact]
    public async Task RefreshTokens_WithStolenToken_ShouldReturnUnauthorized()
    {
        var (user, password) = _userFactory.CreateUser();
        var loginResponse = await BaseClient.Login(user.Username, password);
        var stolenToken = loginResponse.Headers.GetValues("Set-Cookie").First();
        CreateClient();

        Clients[1].DefaultRequestHeaders.Add("User-Agent", "Bad person agent");
        Clients[1].DefaultRequestHeaders.Add("Cookie", stolenToken);
        var response = await Clients[1].RefreshTokens();
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}