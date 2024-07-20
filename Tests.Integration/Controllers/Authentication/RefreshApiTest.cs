using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Api;
using SafariDigital.Database.Context;
using SafariDigital.Database.Models.User;
using SafariDigital.Database.Repository;
using SafariDigital.Services.Authentication.Models;
using Tests.Core.Factories;
using Tests.Core.Integration;
using Tests.Core.Utils;

namespace Tests.Integration.Controllers.Authentication;

public class RefreshApiTest : IntegrationTest
{
    private const string LoginApi = "/authentication/login";
    private const string RefreshApi = "/authentication/refresh";
    private readonly Repository<User> _userRepository;

    public RefreshApiTest(ApiFactory<Program> fixture) : base(fixture)
    {
        _userRepository =
            new Repository<User>(Factory.Services.GetRequiredService<SafariDigitalContext>());
    }

    [Fact]
    public async Task RefreshTokens_ShouldReturnToken()
    {
        // Arrange
        var user = Setup();
        var loginResponse = await Client.PostAsJsonAsync(
            LoginApi,
            new LoginRequest(user.Username, user.Password)
        );
        await Client.SetAuthorizations(loginResponse);

        // Act
        var response = await Client.GetAsync(RefreshApi);
        response.EnsureSuccessStatusCode();

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("token", content);
    }

    [Fact]
    public async Task RefreshTokens_ShouldReturnUnauthorized()
    {
        // Arrange
        var user = Setup();
        var loginResponse = await Client.PostAsJsonAsync(
            LoginApi,
            new LoginRequest(user.Username, user.Password)
        );
        var stolenToken = loginResponse.Headers.GetValues("Set-Cookie").First();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Bad person agent");

        // Act
        client.DefaultRequestHeaders.Add("Cookie", stolenToken);
        var response = await client.GetAsync(RefreshApi);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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