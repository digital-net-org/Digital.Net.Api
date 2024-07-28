using System.Net;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Api;
using SafariDigital.Database.Context;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Database.Repository;
using Tests.Core.Factories;
using Tests.Core.Integration;
using Tests.Integration.TestUtils.ApiCollections;

namespace Tests.Integration.Controllers.AuthenticationController;

public class RefreshApiTest : IntegrationTest
{
    private readonly Repository<User> _userRepository;

    public RefreshApiTest(ApiFactory<Program> fixture) : base(fixture)
    {
        _userRepository =
            new Repository<User>(Factory.Services.GetRequiredService<SafariDigitalContext>());
    }

    [Fact]
    public async Task RefreshTokens_WithValidRefreshToken_ShouldReturnToken()
    {
        var user = Setup();
        await Client.Login(user.Username, user.Password);
        var response = await Client.RefreshTokens();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("token", content);
    }

    [Fact]
    public async Task RefreshTokens_WithStolenToken_ShouldReturnUnauthorized()
    {
        var user = Setup();
        var loginResponse = await Client.Login(user.Username, user.Password);
        var stolenToken = loginResponse.Headers.GetValues("Set-Cookie").First();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Bad person agent");
        client.DefaultRequestHeaders.Add("Cookie", stolenToken);
        var response = await client.RefreshTokens();
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