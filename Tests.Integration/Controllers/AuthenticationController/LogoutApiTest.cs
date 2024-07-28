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

public class LogoutApiTest : IntegrationTest
{
    private readonly Repository<User> _userRepository;

    public LogoutApiTest(ApiFactory<Program> fixture) : base(fixture)
    {
        _userRepository =
            new Repository<User>(Factory.Services.GetRequiredService<SafariDigitalContext>());
    }

    [Fact]
    public async Task Logout_ShouldLogoutClient()
    {
        var user = Setup();
        await Client.Login(user.Username, user.Password);
        var logoutResponse = await Client.Logout();
        var cookies = Client.DefaultRequestHeaders.GetValues("Cookie");
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        Assert.DoesNotContain("RefreshToken", cookies);
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