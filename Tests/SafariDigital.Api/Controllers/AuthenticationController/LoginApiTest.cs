using System.Net;
using Safari.Net.TestTools.Integration;
using SafariDigital.Api;
using SafariDigital.Core.Application;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models.Database;
using Tests.Utils.ApiCollections;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Api.Controllers.AuthenticationController;

public class LoginApiTest : IntegrationTest<Program, SafariDigitalContext>
{
    private readonly UserFactory _userFactory;

    public LoginApiTest(AppFactory<Program, SafariDigitalContext> fixture) : base(fixture)
    {
        SafariDigitalRepository<User> userRepository = new(GetContext());
        _userFactory = new UserFactory(userRepository);
    }

    [Fact]
    public async Task Login_ShouldReturnToken()
    {
        var (user, password) = _userFactory.CreateUser();
        var response = await BaseClient.Login(user.Username, password);
        var content = await response.Content.ReadAsStringAsync();
        var refreshToken = response.Headers.GetValues("Set-Cookie").First();
        Assert.Contains("token", content);
        Assert.NotNull(refreshToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorizedOnWrongPassword()
    {
        var (user, _) = _userFactory.CreateUser();
        var response = await BaseClient.Login(user.Username, "wrongPassword");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorizedOnInactiveUser()
    {
        var (user, password) = _userFactory.CreateUser(new UserPayload { IsActive = false });
        var response = await BaseClient.Login(user.Username, password);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldAllowOnlyXTokenPerUser()
    {
        var allowedConnections = Configuration.GetSection<int>(EApplicationSetting.JwtMaxTokenAllowed);
        var (user, password) = _userFactory.CreateUser();
        var responses = new List<HttpResponseMessage>();

        CreateClient(allowedConnections + 1);
        foreach (var client in Clients)
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"Client {Clients.IndexOf(client)}");
            await client.Login(user.Username, password);
            responses.Add(await client.TestUserAuthorization());
        }

        var unauthorizedResponse = await BaseClient.RefreshTokens();
        foreach (var response in responses)
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);
    }
}