using System.Net;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Api;
using SafariDigital.Core.Application;
using SafariDigital.Database.Context;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Database.Repository;
using Tests.Core.Factories;
using Tests.Core.Integration;
using Tests.Integration.TestUtils.ApiCollections;

namespace Tests.Integration.Controllers.AuthenticationController;

public class LoginApiTest : IntegrationTest
{
    private readonly Repository<User> _userRepository;

    public LoginApiTest(ApiFactory<Program> fixture) : base(fixture)
    {
        _userRepository =
            new Repository<User>(Factory.Services.GetRequiredService<SafariDigitalContext>());
    }

    [Fact]
    public async Task Login_ShouldReturnToken()
    {
        var user = Setup();
        var response = await Client.Login(user.Username, user.Password);
        var content = await response.Content.ReadAsStringAsync();
        var refreshToken = response.Headers.GetValues("Set-Cookie").First();
        Assert.Contains("token", content);
        Assert.NotNull(refreshToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorizedOnWrongPassword()
    {
        var user = Setup();
        var response = await Client.Login(user.Username, "wrongPassword");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorizedOnInactiveUser()
    {
        var user = Setup(UserFactory.CreateUser(isActive: false));
        var response = await Client.Login(user.Username, user.Password);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldAllowOnlyXTokenPerUser()
    {
        var allowedConnections = Configuration.GetSectionOrThrow<int>(EApplicationSetting.JwtMaxTokenAllowed);
        var user = Setup();
        var httpClients = new List<HttpClient>();
        var responses = new List<HttpResponseMessage>();

        // +1 As the first client will be disconnected on the exceeding request
        for (var i = 0; i < allowedConnections + 1; i++)
        {
            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"Client {i}");
            httpClients.Add(client);
        }

        foreach (var client in httpClients)
        {
            await client.Login(user.Username, user.Password);
            responses.Add(await client.TestUserAuthorization());
        }

        var unauthorizedResponse = await httpClients[0].RefreshTokens();
        foreach (var response in responses) Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);
    }

    private User Setup(User? user = null)
    {
        user ??= UserFactory.CreateUser();
        _userRepository.Create(UserFactory.CreateUserWithHashedPassword(user));
        _userRepository.Save();
        var retrieved = _userRepository.Get(u => u.Username == user.Username).FirstOrDefault()!;
        retrieved.Password = user.Password;
        return retrieved;
    }
}