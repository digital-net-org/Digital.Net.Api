using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Api;
using SafariDigital.Core.Application;
using SafariDigital.Database.Context;
using SafariDigital.Database.Models.User;
using SafariDigital.Database.Repository;
using SafariDigital.Services.Authentication.Models;
using Tests.Core.Factories;
using Tests.Core.Integration;
using Tests.Core.Utils;

namespace Tests.Integration.Controllers.Authentication;

public class LoginApiTest : IntegrationTest
{
    private const string Api = "/authentication/login";
    private const string RefreshApi = "/authentication/refresh";
    private const string RoleTestApi = "/authentication/role/user/test";
    private readonly Repository<User> _userRepository;

    public LoginApiTest(ApiFactory<Program> fixture) : base(fixture)
    {
        _userRepository =
            new Repository<User>(Factory.Services.GetRequiredService<SafariDigitalContext>());
    }

    [Fact]
    public async Task Login_ShouldReturnToken()
    {
        // Arrange
        var user = Setup();

        // Act
        var response = await Client.PostAsJsonAsync(Api, new LoginRequest(user.Username, user.Password));
        response.EnsureSuccessStatusCode();

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var refreshToken = response.Headers.GetValues("Set-Cookie").First();

        Assert.Contains("token", content);
        Assert.NotNull(refreshToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorizedOnWrongPassword()
    {
        // Arrange
        var user = Setup();
        // Act
        var response = await Client.PostAsJsonAsync(Api, new LoginRequest(user.Username, "wrongPassword"));
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorizedOnInactiveUser()
    {
        // Arrange
        var user = Setup(UserFactory.CreateUser(isActive: false));
        // Act
        var response = await Client.PostAsJsonAsync(Api, new LoginRequest(user.Username, user.Password));
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldAllowOnlyXTokenPerUser()
    {
        // Arrange
        var allowedConnections = Configuration.GetSettingOrThrow<int>(EApplicationSetting.JwtMaxTokenAllowed);
        var user = Setup();
        var httpClients = new List<HttpClient>();
        var responses = new List<HttpResponseMessage>();
        for (var i = 0; i < allowedConnections + 1; i++)
        {
            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"Client {i}");
            httpClients.Add(client);
        }

        // Act
        foreach (var client in httpClients)
        {
            var loginResponse = await client.PostAsJsonAsync(Api, new LoginRequest(user.Username, user.Password));
            await client.SetAuthorizations(loginResponse);
            var response = await client.GetAsync(RoleTestApi);
            responses.Add(response);
        }

        var unauthorizedResponse = await httpClients[0].GetAsync(RefreshApi);

        // Assert
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